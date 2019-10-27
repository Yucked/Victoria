using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Socks;
using Socks.EventArgs;
using Victoria.Converters;
using Victoria.EventArgs;
using Victoria.Payloads;
using Victoria.Responses.Rest;
using Victoria.Responses.WebSocket;

namespace Victoria
{
    /// <inheritdoc />
    public class LavaNode : LavaNode<LavaPlayer>
    {
        /// <inheritdoc />
        public LavaNode(DiscordSocketClient socketClient, LavaConfig config) : base(socketClient, config)
        {
        }

        /// <inheritdoc />
        public LavaNode(DiscordShardedClient shardedClient, LavaConfig config) : base(shardedClient, config)
        {
        }
    }

    /// <summary>
    /// </summary>
    public class LavaNode<TPlayer> : IAsyncDisposable
        where TPlayer : LavaPlayer

    {
        /// <summary>
        ///     Fires when a player update is received.
        /// </summary>
        public event Func<PlayerUpdateEventArgs, Task> OnPlayerUpdated;

        /// <summary>
        ///     Fires when Lavalink server sends statistics.
        /// </summary>
        public event Func<StatsEventArgs, Task> OnStatsReceived;

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

        /// <summary>
        ///     Checks if the client has an active WebSocket connection.
        /// </summary>
        public bool IsConnected
            => Volatile.Read(ref _refConnected);

        /// <summary>
        ///     Collection of <see cref="TPlayer" />.
        /// </summary>
        public IEnumerable<TPlayer> Players
            => _playerCache.Values;

        private bool _refConnected;

        private readonly LavaConfig _config;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly BaseSocketClient _socketClient;
        private readonly ClientSock _sock;
        private readonly ConcurrentDictionary<ulong, TPlayer> _playerCache;
        private readonly HttpClient _httpClient;

        /// <inheritdoc />
        public LavaNode(DiscordSocketClient socketClient, LavaConfig config)
            : this(socketClient as BaseSocketClient, config)
        {
        }

        /// <inheritdoc />
        public LavaNode(DiscordShardedClient shardedClient, LavaConfig config)
            : this(shardedClient as BaseSocketClient, config)
        {
        }

        private LavaNode(BaseSocketClient socketClient, LavaConfig config)
        {
            _config = config;

            _socketClient = socketClient;
            socketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
            socketClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;


            _sock = new ClientSock(new SockConfig
            {
                Endpoint = new Endpoint(config.Hostname, config.Port, false),
                BufferSize = config.BufferSize,
                ReconnectSettings = new ReconnectSettings
                {
                    Interval = config.ReconnectDelay,
                    MaximumAttempts = config.ReconnectAttempts
                }
            });

            _sock.OnRetry += OnRetryAsync;
            _sock.OnReceive += OnReceiveAsync;
            _sock.OnConnected += OnConnectedAsync;
            _sock.OnDisconnected += OnDisconnectedAsync;

            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(new SearchResponseConverter());
            _jsonOptions.Converters.Add(new WebsocketResponseConverter());

            _playerCache = new ConcurrentDictionary<ulong, TPlayer>();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{config.Hostname}:{config.Port}")
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", _config.Authorization);
        }

        /// <summary>
        ///     Starts a WebSocket connection to the specified <see cref="LavaConfig.Hostname" />:<see cref="LavaConfig.Port" />
        ///     and hooks into <see cref="BaseSocketClient" /> events.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if client is already connected.</exception>
        public async Task ConnectAsync()
        {
            if (Volatile.Read(ref _refConnected))
                throw new InvalidOperationException(
                    $"You must call {nameof(DisconnectAsync)} or {nameof(DisposeAsync)} before calling {nameof(ConnectAsync)}.");

            if (_socketClient?.CurrentUser == null || _socketClient.CurrentUser.Id == 0)
                throw new InvalidOperationException($"{nameof(_socketClient)} is not in ready state.");

            var shards = _socketClient switch
            {
                DiscordSocketClient socketClient => await socketClient.GetRecommendedShardCountAsync()
                    .ConfigureAwait(false),
                DiscordShardedClient shardedClient => shardedClient.Shards.Count,
                _                                  => 1
            };

            _sock.AddHeader("User-Id", $"{_socketClient.CurrentUser.Id}");
            _sock.AddHeader("Num-Shards", $"{shards}");
            _sock.AddHeader("Authorization", _config.Authorization);

            if (_config.EnableResume)
                _sock.AddHeader("Resume-Key", _config.ResumeKey);

            await _sock.ConnectAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Disposes all players and closes websocket connection.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
        public async Task DisconnectAsync()
        {
            if (!Volatile.Read(ref _refConnected))
                throw new InvalidOperationException("Can't disconnect when client isn't connected.");

            foreach (var (_, value) in _playerCache)
                await value.DisposeAsync()
                    .ConfigureAwait(false);

            _playerCache.Clear();

            await _sock.DisconnectAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Joins the specified voice channel and returns the connected <see cref="TPlayer" />.
        /// </summary>
        /// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
        /// <param name="textChannel">An instance of <see cref="ITextChannel" />.</param>
        /// <returns>
        ///     <see cref="TPlayer"/>
        /// </returns>
        public async Task<TPlayer> JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = default)
        {
            if (voiceChannel == null)
                throw new ArgumentNullException(nameof(voiceChannel));

            if (_playerCache.TryGetValue(voiceChannel.GuildId, out var player))
                return player;

            await voiceChannel.ConnectAsync(_config.SelfDeaf, false, true)
                .ConfigureAwait(false);

            player = (TPlayer) Activator.CreateInstance(typeof(TPlayer), _sock, voiceChannel, textChannel);
            _playerCache.TryAdd(voiceChannel.GuildId, player);
            return player;
        }

        /// <summary>
        ///     Moves from one voice channel to another.
        /// </summary>
        /// <param name="voiceChannel">Voice channel to connect to.</param>
        /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
        public async Task MoveAsync(IVoiceChannel voiceChannel)
        {
            if (voiceChannel == null)
                throw new ArgumentNullException(nameof(voiceChannel));

            if (!Volatile.Read(ref _refConnected))
                throw new InvalidOperationException("Can't execute this operation when client isn't connected.");

            if (!_playerCache.TryGetValue(voiceChannel.GuildId, out var player))
                throw new InvalidOperationException($"No player was found for {voiceChannel.Guild.Name}.");

            if (player.VoiceChannel.Id == voiceChannel.Id)
                throw new InvalidOperationException("Connected and new voice channel ids are the same.");

            var payload = new DestroyPayload(voiceChannel.GuildId);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);

            await player.VoiceChannel.DisconnectAsync()
                .ConfigureAwait(false);

            await voiceChannel.ConnectAsync(_config.SelfDeaf, false, true)
                .ConfigureAwait(false);

            Log(LogSeverity.Info, $"Moved {voiceChannel.GuildId} player to {voiceChannel.Name}.");
        }

        /// <summary>
        ///     Leaves the specified channel only if <see cref="TPlayer" /> is connected to it.
        /// </summary>
        /// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
        /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
        public async Task LeaveAsync(IVoiceChannel voiceChannel)
        {
            if (!Volatile.Read(ref _refConnected))
                throw new InvalidOperationException("Can't execute this operation when client isn't connected.");

            if (!_playerCache.TryGetValue(voiceChannel.GuildId, out var player))
                return;

            await player.DisposeAsync()
                .ConfigureAwait(false);

            await voiceChannel.DisconnectAsync()
                .ConfigureAwait(false);

            _playerCache.TryRemove(voiceChannel.GuildId, out _);
        }

        /// <summary>
        ///     Checks if <see cref="TPlayer" /> exists for specified guild.
        /// </summary>
        /// <param name="guild">An instance of <see cref="IGuild" />.</param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool HasPlayer(IGuild guild)
            => _playerCache.ContainsKey(guild.Id);

        /// <summary>
        /// Returns the player for the specified guild.
        /// </summary>
        /// <param name="guild">An instance of <see cref="IGuild"/>.</param>
        /// <returns><see cref="bool"/></returns>
        public TPlayer GetPlayer(IGuild guild)
            => _playerCache[guild.Id];

        /// <summary>
        ///     Returns either an existing or null player.
        /// </summary>
        /// <param name="guild">An instance of <see cref="IGuild" />.</param>
        /// <param name="player">An instance of <see cref="TPlayer" /></param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool TryGetPlayer(IGuild guild, out TPlayer player)
            => _playerCache.TryGetValue(guild.Id, out player);

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync()
                .ConfigureAwait(false);

            await _sock.DisposeAsync()
                .ConfigureAwait(false);

            _playerCache.Clear();
        }

        /// <summary>
        ///     Searches YouTube for your query.
        /// </summary>
        /// <param name="query">Your search terms.</param>
        /// <returns>
        ///     <see cref="SearchResponse" />
        /// </returns>
        public Task<SearchResponse> SearchYouTubeAsync(string query)
            => SearchAsync($"ytsearch:{query}");

        /// <summary>
        ///     Searches SoundCloud for your query.
        /// </summary>
        /// <param name="query">Your search terms.</param>
        /// <returns>
        ///     <see cref="SearchResponse" />
        /// </returns>
        public Task<SearchResponse> SearchSoundCloudAsync(string query)
            => SearchAsync($"scsearch:{query}");

        /// <summary>
        ///     Performs search on all enabled sources in configuration.
        /// </summary>
        /// <param name="query">Your search terms.</param>
        /// <returns>
        ///     <see cref="SearchResponse" />
        /// </returns>
        public async Task<SearchResponse> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/loadtracks?identifier={WebUtility.UrlEncode(query)}");

            var response = await _httpClient.SendAsync(request)
                .ConfigureAwait(false);

            var content = await response.Content.ReadAsByteArrayAsync()
                .ConfigureAwait(false);

            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(content.AsSpan(), _jsonOptions);
            return searchResponse;
        }

        private async Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldState,
            SocketVoiceState newState)
        {
            if (user.Id != _socketClient.CurrentUser.Id)
                return;

            var guildId = newState.VoiceChannel?.Guild.Id;

            if (!_playerCache.TryGetValue(guildId.GetValueOrDefault(), out var player))
                return;

            player.VoiceState = newState;

            await Task.Delay(0)
                .ConfigureAwait(false);
        }

        private async Task OnVoiceServerUpdatedAsync(SocketVoiceServer voiceServer)
        {
            if (!_playerCache.TryGetValue(voiceServer.Guild.Id, out var player))
                return;

            var payload = new ServerUpdatePayload
            {
                SessionId = player.VoiceState.VoiceSessionId,
                GuildId = $"{voiceServer.Guild.Id}",
                VoiceServerPayload = new VoiceServerPayload
                {
                    Token = voiceServer.Token, Endpoint = voiceServer.Endpoint
                }
            };

            await _sock.SendAsync(payload)
                .ConfigureAwait(false);
        }

        private Task OnRetryAsync(RetryEventArgs args)
        {
            Log(LogSeverity.Warning, args.Message);
            return Task.CompletedTask;
        }

        private async Task OnConnectedAsync()
        {
            Volatile.Write(ref _refConnected, true);
            Log(LogSeverity.Info, "Websocket connection established.");

            if (_config.EnableResume)
            {
                var payload = new ResumePayload(_config.ResumeKey, _config.ResumeTimeout);
                await _sock.SendAsync(payload)
                    .ConfigureAwait(false);
            }

            await Task.Delay(0)
                .ConfigureAwait(false);
        }

        private async Task OnDisconnectedAsync(DisconnectEventArgs eventArgs)
        {
            Volatile.Write(ref _refConnected, false);
            Log(LogSeverity.Info, eventArgs.Message ?? eventArgs.Exception.Message);
            await Task.Delay(0)
                .ConfigureAwait(false);
        }

        private async Task OnReceiveAsync(ReceivedEventArgs eventArgs)
        {
            if (eventArgs.DataSize == 0)
            {
                Log(LogSeverity.Warning, "Received empty payload from Lavalink.");
                return;
            }

            Log(LogSeverity.Debug, eventArgs.Raw);

            var baseWsResponse = JsonSerializer.Deserialize<BaseWsResponse>(eventArgs.Data.Span, _jsonOptions);

            switch (baseWsResponse)
            {
                case PlayerUpdateResponse playerUpdateResponse:
                    if (!_playerCache.TryGetValue(playerUpdateResponse.GuildId, out var player))
                        return;

                    OnPlayerUpdated?.Invoke(new PlayerUpdateEventArgs(player, playerUpdateResponse));
                    break;

                case StatsResponse statsResponse:
                    OnStatsReceived?.Invoke(new StatsEventArgs(statsResponse));
                    break;

                case BaseEventResponse eventResponse:
                    switch (eventResponse)
                    {
                        case TrackEndEvent trackEndEvent:
                            if (!_playerCache.TryGetValue(trackEndEvent.GuildId, out player))
                                return;

                            OnTrackEnded?.Invoke(new TrackEndedEventArgs(player, trackEndEvent));
                            break;

                        case TrackStuckEvent trackStuckEvent:
                            if (!_playerCache.TryGetValue(trackStuckEvent.GuildId, out player))
                                return;

                            OnTrackStuck?.Invoke(new TrackStuckEventArgs(player, trackStuckEvent));
                            break;

                        case TrackExceptionEvent trackExceptionEvent:
                            if (!_playerCache.TryGetValue(trackExceptionEvent.GuildId, out player))
                                return;

                            OnTrackException?.Invoke(new TrackExceptionEventArgs(player, trackExceptionEvent));
                            break;

                        case WebSocketClosedEvent socketClosedEvent:
                            OnWebSocketClosed?.Invoke(new WebSocketClosedEventArgs(socketClosedEvent));
                            break;
                    }

                    break;
            }

            await Task.Delay(0)
                .ConfigureAwait(false);
        }

        private void Log(LogSeverity severity, string message, Exception exception = null)
        {
            if (severity > _config.LogSeverity)
                return;

            var logMessage = new LogMessage(severity, nameof(Victoria), message, exception);
            OnLog?.Invoke(logMessage)
                .ConfigureAwait(false);
        }
    }
}