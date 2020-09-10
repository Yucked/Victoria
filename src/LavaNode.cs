using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria.Converters;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Payloads;
using Victoria.Responses.Rest;
using Victoria.Responses.WebSocket;
using PlayerState = Victoria.Enums.PlayerState;

namespace Victoria {
	/// <summary>
	/// Represents a single connection to a Lavalink server.
	/// </summary>
	public class LavaNode : LavaNode<LavaPlayer> {
		/// <inheritdoc />
		public LavaNode(DiscordSocketClient socketClient, LavaConfig config) : base(socketClient, config) {
		}

		/// <inheritdoc />
		public LavaNode(DiscordShardedClient shardedClient, LavaConfig config) : base(shardedClient, config) {
		}
	}

	/// <summary>
	///     Represents a single connection to a Lavalink server with custom <typeparamref name="TPlayer"/>.
	/// </summary>
	/// <typeparam name="TPlayer">Where TPlayer is inherited from <see cref="LavaPlayer" /></typeparam>.
    public class LavaNode<TPlayer> : IAsyncDisposable
		where TPlayer : LavaPlayer {
		/// <summary>
		///     Checks if the client has an active WebSocket connection.
		/// </summary>
		public bool IsConnected
			=> Volatile.Read(ref _refConnected);

		/// <summary>
		///     Collection of <typeparamref name="TPlayer" />.
		/// </summary>
		public IEnumerable<TPlayer> Players
			=> _playerCache.Values;

		/// <summary>
		///     IP rotation extension.
		/// </summary>
		public RoutePlanner RoutePlanner { get; }

		private readonly LavaConfig _config;
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions _jsonOptions;
		private readonly LavaSocket _lavaSocket;
		private readonly ConcurrentDictionary<ulong, TPlayer> _playerCache;
		private readonly BaseSocketClient _socketClient;

		private bool _refConnected;

		/// <summary>
		///     Fires when a player update is received.
		/// </summary>
		public event Func<PlayerUpdateEventArgs, Task> OnPlayerUpdated;

		/// <summary>
		///     Fires when Lavalink server sends statistics.
		/// </summary>
		public event Func<StatsEventArgs, Task> OnStatsReceived;

		/// <summary>
		///     Fires when a track playback has started.
		/// </summary>
		public event Func<TrackStartEventArgs, Task> OnTrackStarted;

		/// <summary>
		///     Fires when a track playback has finished.
		/// </summary>
		public event Func<TrackEndedEventArgs, Task> OnTrackEnded;

		/// <summary>
		///     Fires when a track has thrown an exception.
		/// </summary>
		public event Func<TrackExceptionEventArgs, Task> OnTrackException;

		/// <summary>
		///     Fires when a track got stuck.
		/// </summary>
		public event Func<TrackStuckEventArgs, Task> OnTrackStuck;

		/// <summary>
		///     Fires when Discord closes the audio WebSocket connection.
		/// </summary>
		public event Func<WebSocketClosedEventArgs, Task> OnWebSocketClosed;

		/// <summary>
		///     Fires whenever a log message is sent.
		/// </summary>
		public event Func<LogMessage, Task> OnLog;

		/// <inheritdoc />
		public LavaNode(DiscordSocketClient socketClient, LavaConfig config)
			: this(socketClient as BaseSocketClient, config) {
		}

		/// <inheritdoc />
		public LavaNode(DiscordShardedClient shardedClient, LavaConfig config)
			: this(shardedClient as BaseSocketClient, config) {
		}

		private LavaNode(BaseSocketClient socketClient, LavaConfig config) {
			_config = config;

			_socketClient = socketClient;
			socketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
			socketClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;

			_lavaSocket = new LavaSocket(config);
			_lavaSocket.OnRetry += OnRetryAsync;
			_lavaSocket.OnReceive += OnReceiveAsync;
			_lavaSocket.OnConnected += OnConnectedAsync;
			_lavaSocket.OnDisconnected += OnDisconnectedAsync;

			_jsonOptions = new JsonSerializerOptions();
			_jsonOptions.Converters.Add(new SearchResponseConverter());
			_jsonOptions.Converters.Add(new WebsocketResponseConverter());

			_playerCache = new ConcurrentDictionary<ulong, TPlayer>();

			_httpClient = new HttpClient {
				BaseAddress = new Uri($"{(_config.IsSSL ? "https" : "http")}://{config.Hostname}:{config.Port}")
			};
			_httpClient.DefaultRequestHeaders.Add("Authorization", _config.Authorization);
			if (!string.IsNullOrWhiteSpace(config.UserAgent))
			{
				_httpClient.DefaultRequestHeaders.Add("User-Agent", _config.UserAgent);
			}

			RoutePlanner = new RoutePlanner(_httpClient);
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync() {
			await DisconnectAsync()
			   .ConfigureAwait(false);

			await _lavaSocket.DisposeAsync()
			   .ConfigureAwait(false);

			_playerCache.Clear();
		}

		/// <summary>
		///     Starts a WebSocket connection to the specified <see cref="LavaConfig.Hostname" />:<see cref="LavaConfig.Port" />
		///     and hooks into <see cref="BaseSocketClient" /> events.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if client is already connected.</exception>
		public async Task ConnectAsync() {
			if (Volatile.Read(ref _refConnected)) {
				throw new InvalidOperationException(
					$"You must call {nameof(DisconnectAsync)} or {nameof(DisposeAsync)} before calling {nameof(ConnectAsync)}.");
			}

			if (_socketClient?.CurrentUser == null || _socketClient.CurrentUser.Id == 0) {
				throw new InvalidOperationException($"{nameof(_socketClient)} is not in ready state.");
			}

			var shards = _socketClient switch {
				DiscordSocketClient socketClient => await socketClient.GetRecommendedShardCountAsync()
				   .ConfigureAwait(false),
				DiscordShardedClient shardedClient => shardedClient.Shards.Count,
				_                                  => 1
			};

			_lavaSocket.SetHeader("User-Id", $"{_socketClient.CurrentUser.Id}");
			_lavaSocket.SetHeader("Num-Shards", $"{shards}");
			_lavaSocket.SetHeader("Authorization", _config.Authorization);

			if (_config.EnableResume) {
				_lavaSocket.SetHeader("Resume-Key", _config.ResumeKey);
			}
			if (!string.IsNullOrWhiteSpace(_config.UserAgent))
			{
				_lavaSocket.SetHeader("User-Agent", _config.UserAgent);
			}

			await _lavaSocket.ConnectAsync()
			   .ConfigureAwait(false);
		}

		/// <summary>
		///     Disposes all players and closes websocket connection.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
		public async Task DisconnectAsync() {
			if (!Volatile.Read(ref _refConnected)) {
				throw new InvalidOperationException("Can't disconnect when client isn't connected.");
			}

			foreach (var (_, value) in _playerCache) {
				await value.DisposeAsync()
				   .ConfigureAwait(false);
			}

			_playerCache.Clear();

			await _lavaSocket.DisconnectAsync()
			   .ConfigureAwait(false);
		}

		/// <summary>
		///     Joins the specified voice channel and returns the connected <typeparamref name="TPlayer" />.
		/// </summary>
		/// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
		/// <param name="textChannel">An instance of <see cref="ITextChannel" />.</param>
		/// <returns>
		///     <typeparamref name="TPlayer" />
		/// </returns>
		public async Task<TPlayer> JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = default) {
			if (voiceChannel == null) {
				throw new ArgumentNullException(nameof(voiceChannel));
			}

			if (_playerCache.TryGetValue(voiceChannel.GuildId, out var player)) {
				return player;
			}

			await voiceChannel.ConnectAsync(_config.SelfDeaf, false, true)
			   .ConfigureAwait(false);

			player = (TPlayer) Activator.CreateInstance(typeof(TPlayer), _lavaSocket, voiceChannel, textChannel);
			_playerCache.TryAdd(voiceChannel.GuildId, player);
			return player;
		}

		/// <summary>
		///     Leaves the specified channel only if <typeparamref name="TPlayer" /> is connected to it.
		/// </summary>
		/// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
		/// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
		public async Task LeaveAsync(IVoiceChannel voiceChannel) {
			if (!Volatile.Read(ref _refConnected)) {
				throw new InvalidOperationException("Can't execute this operation when client isn't connected.");
			}

			if (!_playerCache.TryGetValue(voiceChannel.GuildId, out var player)) {
				return;
			}

			await player.DisposeAsync()
			   .ConfigureAwait(false);

			await voiceChannel.DisconnectAsync()
			   .ConfigureAwait(false);

			_playerCache.TryRemove(voiceChannel.GuildId, out _);
		}

		/// <summary>
		///     Checks if <typeparamref name="TPlayer" /> exists for specified guild.
		/// </summary>
		/// <param name="guild">An instance of <see cref="IGuild" />.</param>
		/// <returns>
		///     <see cref="bool" />
		/// </returns>
		public bool HasPlayer(IGuild guild) {
			return _playerCache.ContainsKey(guild.Id);
		}

		/// <summary>
		///     Returns the player for the specified guild.
		/// </summary>
		/// <param name="guild">An instance of <see cref="IGuild" />.</param>
		/// <returns>
		///     <see cref="bool" />
		/// </returns>
		public TPlayer GetPlayer(IGuild guild) {
			return _playerCache[guild.Id];
		}

		/// <summary>
		///     Returns either an existing or null player.
		/// </summary>
		/// <param name="guild">An instance of <see cref="IGuild" />.</param>
		/// <param name="player">An instance of <typeparamref name="TPlayer" /></param>
		/// <returns>
		///     <see cref="bool" />
		/// </returns>
		public bool TryGetPlayer(IGuild guild, out TPlayer player) {
			return _playerCache.TryGetValue(guild.Id, out player);
		}

		/// <summary>
		///     Moves either a voice channel or text channel.
		/// </summary>
		/// <param name="channel">An instance of <see cref="IChannel" />.</param>
		/// <typeparam name="T"></typeparam>
		/// <exception cref="InvalidOperationException">Throws if client isn't connected or if player doesn't exist in cache.</exception>
		/// <exception cref="ArgumentNullException">Throws if channel is null.</exception>
		/// <exception cref="ArgumentException">
		///     Throws if channel isn't an <see cref="IVoiceChannel" /> or
		///     <see cref="ITextChannel" />.
		/// </exception>
		public async Task MoveChannelAsync<T>(T channel) where T : IChannel {
			switch (channel) {
				case IVoiceChannel voiceChannel: {
					if (!Volatile.Read(ref _refConnected)) {
						throw new InvalidOperationException(
							"Can't execute this operation when client isn't connected.");
					}

					if (!_playerCache.TryGetValue(voiceChannel.GuildId, out var player)) {
						throw new InvalidOperationException($"No player was found for {voiceChannel.Guild.Name}.");
					}

					if (player.VoiceChannel.Id == voiceChannel.Id) {
						throw new InvalidOperationException("Connected and new voice channel ids are the same.");
					}

					if (player.PlayerState == PlayerState.Playing) {
						await player.PauseAsync()
						   .ConfigureAwait(false);
					}

					player.VoiceChannel = voiceChannel;
					await voiceChannel.ConnectAsync(_config.SelfDeaf, false, true)
					   .ConfigureAwait(false);

					if (player.PlayerState == PlayerState.Paused) {
						await player.ResumeAsync();
					}

					Log(LogSeverity.Info, $"Moved {voiceChannel.GuildId} player to {voiceChannel.Name}.");
					break;
				}

				case ITextChannel textChannel: {
					if (!_playerCache.TryGetValue(textChannel.Guild.Id, out var player)) {
						throw new InvalidOperationException("Player doesn't exist in cache.");
					}

					player.TextChannel = textChannel;
					break;
				}

				case null: {
					throw new ArgumentNullException(nameof(channel), "Channel cannot be null.");
				}

				default: {
					throw new ArgumentException("Channel must be an IVoiceChannel, ITextChannel.", nameof(channel));
				}
			}
		}

		/// <summary>
		///     Searches YouTube for your query.
		/// </summary>
		/// <param name="query">Your search terms.</param>
		/// <returns>
		///     <see cref="SearchResponse" />
		/// </returns>
		public Task<SearchResponse> SearchYouTubeAsync(string query) {
			return SearchAsync($"ytsearch:{query}");
		}

		/// <summary>
		///     Searches SoundCloud for your query.
		/// </summary>
		/// <param name="query">Your search terms.</param>
		/// <returns>
		///     <see cref="SearchResponse" />
		/// </returns>
		public Task<SearchResponse> SearchSoundCloudAsync(string query) {
			return SearchAsync($"scsearch:{query}");
		}

		/// <summary>
		///     Performs search on all enabled sources in configuration.
		/// </summary>
		/// <param name="query">Your search terms.</param>
		/// <returns>
		///     <see cref="SearchResponse" />
		/// </returns>
		public async Task<SearchResponse> SearchAsync(string query) {
			if (string.IsNullOrWhiteSpace(query)) {
				throw new ArgumentNullException(nameof(query));
			}

			using var request = new HttpRequestMessage(HttpMethod.Get,
				$"/loadtracks?identifier={WebUtility.UrlEncode(query)}");

			using var response = await _httpClient.SendAsync(request)
			   .ConfigureAwait(false);

			var content = await response.Content.ReadAsByteArrayAsync()
			   .ConfigureAwait(false);

			var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content, _jsonOptions);
			return searchResponse;
		}

		private Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldState,
			SocketVoiceState newState) {
			if (_socketClient?.CurrentUser == null || user.Id != _socketClient.CurrentUser.Id) {
				return Task.CompletedTask;
			}

			var guildId = newState.VoiceChannel?.Guild.Id;

			if (!_playerCache.TryGetValue(guildId.GetValueOrDefault(), out var player)) {
				return Task.CompletedTask;
			}

			player.VoiceState = newState;
			player.VoiceChannel = newState.VoiceChannel;

			return Task.CompletedTask;
		}

		private async Task OnVoiceServerUpdatedAsync(SocketVoiceServer voiceServer) {
			if (!_playerCache.TryGetValue(voiceServer.Guild.Id, out var player)) {
				return;
			}

			player.VoiceServer = voiceServer;
			var payload = new ServerUpdatePayload {
				SessionId = player.VoiceState.VoiceSessionId,
				GuildId = $"{voiceServer.Guild.Id}",
				VoiceServerPayload = new VoiceServerPayload {
					Token = voiceServer.Token,
					Endpoint = voiceServer.Endpoint
				}
			};

			await _lavaSocket.SendAsync(payload)
			   .ConfigureAwait(false);
		}

		private Task OnRetryAsync(string retryMessage) {
			Log(LogSeverity.Warning, retryMessage);
			return Task.CompletedTask;
		}

		private async Task OnConnectedAsync() {
			Volatile.Write(ref _refConnected, true);
			Log(LogSeverity.Info, "Websocket connection established.");

			if (_config.EnableResume) {
				var payload = new ResumePayload(_config.ResumeKey, _config.ResumeTimeout);
				await _lavaSocket.SendAsync(payload)
				   .ConfigureAwait(false);
			}
		}

		private Task OnDisconnectedAsync(string disconnectMessage) {
			Volatile.Write(ref _refConnected, false);
			Log(LogSeverity.Error, disconnectMessage);

			return Task.CompletedTask;
		}

		private async Task OnReceiveAsync(byte[] data) {
			if (data.Length == 0) {
				Log(LogSeverity.Warning, "Received empty payload from Lavalink.");
				return;
			}

			Log(LogSeverity.Debug, Encoding.UTF8.GetString(data));
			var baseWsResponse = JsonSerializer.Deserialize<BaseWsResponse>(data, _jsonOptions);

			switch (baseWsResponse) {
				case PlayerUpdateResponse playerUpdateResponse: {
					if (!_playerCache.TryGetValue(playerUpdateResponse.GuildId, out var player)) {
						return;
					}
                    
                    player.Track.Position = playerUpdateResponse.State.Position;
					player.LastUpdate = playerUpdateResponse.State.Time;

					var playerUpdateEventArgs = new PlayerUpdateEventArgs(player, playerUpdateResponse);
					OnPlayerUpdated?.Invoke(playerUpdateEventArgs);
					break;
				}

				case StatsResponse statsResponse: {
					OnStatsReceived?.Invoke(new StatsEventArgs(statsResponse));
					return;
				}

				case BaseEventResponse eventResponse: {
					switch (eventResponse) {
						case TrackStartEvent trackStartEvent: {
							if (!_playerCache.TryGetValue(trackStartEvent.GuildId, out var player)) {
								break;
							}

							player.PlayerState = PlayerState.Playing;
							OnTrackStarted?.Invoke(new TrackStartEventArgs(player, trackStartEvent));
							break;
						}

						case TrackEndEvent trackEndEvent: {
							if (!_playerCache.TryGetValue(trackEndEvent.GuildId, out var player)) {
								break;
							}

							if (trackEndEvent.Reason != TrackEndReason.Replaced) {
								player.Track = default;
								player.PlayerState = PlayerState.Stopped;
							}

							var trackEndedEventArgs = new TrackEndedEventArgs(player, trackEndEvent);
							OnTrackEnded?.Invoke(trackEndedEventArgs);
							break;
						}

						case TrackStuckEvent trackStuckEvent: {
							if (!_playerCache.TryGetValue(trackStuckEvent.GuildId, out var player)) {
								break;
							}

							player.PlayerState = PlayerState.Stopped;
							OnTrackStuck?.Invoke(new TrackStuckEventArgs(player, trackStuckEvent));
							break;
						}

						case TrackExceptionEvent trackExceptionEvent: {
							if (!_playerCache.TryGetValue(trackExceptionEvent.GuildId, out var player)) {
								break;
							}

							player.PlayerState = PlayerState.Stopped;
							OnTrackException?.Invoke(new TrackExceptionEventArgs(player, trackExceptionEvent));
							break;
						}

						case WebSocketClosedEvent socketClosedEvent: {
							OnWebSocketClosed?.Invoke(new WebSocketClosedEventArgs(socketClosedEvent));
							break;
						}
					}

					break;
				}
			}
		}

		private void Log(LogSeverity severity, string message, Exception exception = null) {
			if (severity > _config.LogSeverity) {
				return;
			}

			var logMessage = new LogMessage(severity, nameof(Victoria), message, exception);
			OnLog?.Invoke(logMessage)
			   .ConfigureAwait(false);
		}
	}
}