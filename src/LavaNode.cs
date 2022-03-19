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
using Discord.Net;
using Discord.WebSocket;
using Victoria.Decoder;
using Victoria.EventArgs;
using Victoria.Payloads;
using Victoria.Responses.Search;
using Victoria.Enums;
using System.Net.WebSockets;

namespace Victoria {
    /// <summary>
    /// Represents a single connection to a Lavalink server via <see cref="DiscordSocketClient"/>
    /// </summary>
    public class LavaNode : LavaNode<LavaPlayer> {
        /// <inheritdoc />
        public LavaNode(DiscordSocketClient socketClient, LavaConfig config)
            : base(socketClient, config) { }

        /// <inheritdoc />
        public LavaNode(DiscordShardedClient shardedClient, LavaConfig config)
            : base(shardedClient, config) { }
    }

    /// <summary>
    ///     Represents a single connection to a Lavalink server with custom <typeparamref name="TPlayer"/>.
    /// </summary>
    /// <typeparam name="TPlayer">Where TPlayer is inherited from <see cref="LavaPlayer" /></typeparam>
    /// 
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

        private readonly LavaConfig _config;
        private readonly LavaSocket _lavaSocket;
        private readonly ConcurrentDictionary<ulong, TPlayer> _playerCache;
        private readonly ConcurrentDictionary<ulong, VoiceState> _voiceStates;
        private readonly BaseSocketClient _socketClient;

        private bool _refConnected;

        /// <summary>
        /// Represents a single connection to a Lavalink server with custom <typeparamref name="TPlayer"/>.
        /// </summary>
        /// <param name="socketClient"><seealso cref="DiscordSocketClient"/> or <see cref="DiscordShardedClient"/></param>
        /// <param name="config"><see cref="LavaConfig"/></param>
        internal LavaNode(BaseSocketClient socketClient, LavaConfig config) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _socketClient = socketClient ?? throw new ArgumentNullException(nameof(socketClient));

            socketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
            socketClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;

            _lavaSocket = new LavaSocket(config);
            _lavaSocket.OnRetryAsync += OnRetryAsync;
            _lavaSocket.OnDataAsync += OnDataAsync;
            _lavaSocket.OnOpenAsync += OnOpenAsync;
            _lavaSocket.OnCloseAsync += OnCloseAsync;
            _lavaSocket.OnErrorAsync += OnErrorAsync;

            _playerCache = new ConcurrentDictionary<ulong, TPlayer>();
            _voiceStates = new ConcurrentDictionary<ulong, VoiceState>();
        }

        /// <inheritdoc />
        public LavaNode(DiscordSocketClient socketClient, LavaConfig config)
            : this(socketClient as BaseSocketClient, config) { }

        /// <inheritdoc />
        public LavaNode(DiscordShardedClient shardedClient, LavaConfig config)
            : this(shardedClient as BaseSocketClient, config) { }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            await DisconnectAsync()
                .ConfigureAwait(false);

            await _lavaSocket.DisposeAsync()
                .ConfigureAwait(false);

            _playerCache.Clear();
            GC.SuppressFinalize(this);
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

            if (_socketClient.CurrentUser == null || _socketClient.CurrentUser.Id == 0) {
                throw new InvalidOperationException($"{nameof(_socketClient)} is not in ready state.");
            }

            _lavaSocket.AddHeader("User-Id", $"{_socketClient.CurrentUser.Id}");
            _lavaSocket.AddHeader("Authorization", _config.Authorization);
            _lavaSocket.AddHeader("Client-Name", $"{nameof(Victoria)}/{typeof(LavaNode).Assembly.GetName().Version}");

            if (_config.EnableResume) {
                _lavaSocket.AddHeader("Resume-Key", _config.ResumeKey);
            }

            if (!string.IsNullOrWhiteSpace(_config.UserAgent)) {
                _lavaSocket.AddHeader("User-Agent", _config.UserAgent);
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

            foreach (var (key, _) in _playerCache) {
                _playerCache.TryRemove(key, out var player);
                await LeaveAsync(player.VoiceChannel)
                    .ConfigureAwait(false);

                await player.DisposeAsync()
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
            if (!Volatile.Read(ref _refConnected)) {
                throw new InvalidOperationException("Can't execute this operation when client isn't connected.");
            }

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
            return SearchAsync(SearchType.YouTube, query);
        }

        /// <summary>
        ///     Searches SoundCloud for your query.
        /// </summary>
        /// <param name="query">Your search terms.</param>
        /// <returns>
        ///     <see cref="SearchResponse" />
        /// </returns>
        public Task<SearchResponse> SearchSoundCloudAsync(string query) {
            return SearchAsync(SearchType.SoundCloud, query);
        }

        /// <summary>
        ///     Performs search on all enabled sources in configuration.
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="query">Your search terms.</param>
        /// <returns>
        ///     <see cref="SearchResponse" />
        /// </returns>
        public async Task<SearchResponse> SearchAsync(SearchType searchType, string query) {
            if (string.IsNullOrWhiteSpace(query)) {
                throw new ArgumentNullException(nameof(query));
            }

            var urlPath = searchType switch {
                SearchType.SoundCloud   => $"/loadtracks?identifier={WebUtility.UrlEncode($"scsearch:{query}")}",
                SearchType.YouTubeMusic => $"/loadtracks?identifier={WebUtility.UrlEncode($"ytmsearch:{query}")}",
                SearchType.YouTube      => $"/loadtracks?identifier={WebUtility.UrlEncode($"ytsearch:{query}")}",
                SearchType.Direct       => $"/loadtracks?identifier={query}",
                _                       => $"/loadtracks?identifier={query}"
            };

            using var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, $"{_config.HttpEndpoint}{urlPath}") {
                    Headers = {
                        {"Authorization", _config.Authorization}
                    }
                };

            var searchResponse = await VictoriaExtensions.ReadAsJsonAsync<SearchResponse>(requestMessage);
            return searchResponse;
        }

        private async Task OnOpenAsync() {
            Volatile.Write(ref _refConnected, true);
            Log(LogSeverity.Info, "Websocket connection established.");

            if (_config.EnableResume) {
                var payload = new ResumePayload(_config.ResumeKey, _config.ResumeTimeout);
                await _lavaSocket.SendAsync(payload)
                    .ConfigureAwait(false);
            }
        }

        private Task OnCloseAsync(string disconnectMessage) {
            Volatile.Write(ref _refConnected, false);
            Log(LogSeverity.Error, disconnectMessage);

            return Task.CompletedTask;
        }

        private Task OnErrorAsync(Exception exception) {
            if (exception is WebSocketClosedException or WebSocketException) {
                Volatile.Write(ref _refConnected, false);
            }

            Log(LogSeverity.Error, $"{exception}");
            return Task.CompletedTask;
        }

        private Task OnRetryAsync(int count, TimeSpan delay, bool isLastRetry) {
            if (isLastRetry) {
                Log(LogSeverity.Error, "This was the last try in establishing connection with Lavalink.");
                return Task.CompletedTask;
            }

            Log(LogSeverity.Warning,
                $"Lavalink reconnect attempt #{count}. Waiting {delay.Seconds}s before next attempt.");
            return Task.CompletedTask;
        }

        private async Task OnDataAsync(byte[] data) {
            try {
                if (data.Length == 0) {
                    Log(LogSeverity.Warning, "Didn't receive any data from websocket");
                    return;
                }

                Log(LogSeverity.Debug, Encoding.UTF8.GetString(data));

                using var document = JsonDocument.Parse(data);
                var root = document.RootElement;

                if (!root.TryGetProperty("op", out var opElement)) {
                    Log(LogSeverity.Critical, "Didn't find OP code in payload.");
                    return;
                }

                switch ($"{opElement}") {
                    case "stats": {
                        if (OnStatsReceived == null) {
                            break;
                        }

                        await OnStatsReceived.Invoke(JsonSerializer.Deserialize<StatsEventArgs>(data));
                        break;
                    }

                    case "playerUpdate": {
                        var (guildId, time, position, isConnected) = VictoriaExtensions.GetPlayerUpdate(root);
                        if (!_playerCache.TryGetValue(guildId, out var player)) {
                            break;
                        }

                        player.Track?.UpdatePosition(position);
                        player.LastUpdate = DateTimeOffset.FromUnixTimeMilliseconds(time);
                        player.IsConnected = isConnected;

                        if (OnPlayerUpdated == null) {
                            break;
                        }

                        await OnPlayerUpdated.Invoke(new PlayerUpdateEventArgs(player));
                        break;
                    }

                    case "event": {
                        var guildId = ulong.Parse($"{root.GetProperty("guildId")}");
                        if (!_playerCache.TryGetValue(guildId, out var player)) {
                            break;
                        }

                        LavaTrack lavaTrack = default;
                        if (root.TryGetProperty("track", out var trackElement)) {
                            lavaTrack = TrackDecoder.Decode($"{trackElement}");
                        }

                        switch ($"{root.GetProperty("type")}") {
                            case "TrackStartEvent": {
                                player.Track = lavaTrack;
                                player.PlayerState = PlayerState.Playing;

                                if (OnTrackStarted == null) {
                                    break;
                                }

                                await OnTrackStarted.Invoke(new TrackStartEventArgs(player, lavaTrack));
                                break;
                            }

                            case "TrackEndEvent": {
                                var reason = $"{root.GetProperty("reason")}";
                                if ((TrackEndReason) reason[0] != TrackEndReason.Replaced) {
                                    player.Track = default;
                                    player.PlayerState = PlayerState.Stopped;
                                }

                                if (OnTrackEnded == null) {
                                    break;
                                }

                                await OnTrackEnded.Invoke(
                                    new TrackEndedEventArgs(player, lavaTrack, reason));
                                break;
                            }

                            case "TrackExceptionEvent": {
                                player.Track = default;
                                player.PlayerState = PlayerState.Stopped;

                                if (OnTrackException == null) {
                                    break;
                                }

                                await OnTrackException.Invoke(
                                    new TrackExceptionEventArgs(player, lavaTrack,
                                        root.GetProperty("exception")
                                            .Deserialize<LavaException>()));
                                break;
                            }

                            case "TrackStuckEvent": {
                                player.Track = default;
                                player.PlayerState = PlayerState.Stopped;

                                if (OnTrackStuck == null) {
                                    break;
                                }

                                await OnTrackStuck.Invoke(
                                    new TrackStuckEventArgs(player, lavaTrack,
                                        long.Parse($"{root.GetProperty("thresholdMs")}")));
                                break;
                            }

                            case "WebSocketClosedEvent": {
                                if (OnWebSocketClosed == null) {
                                    break;
                                }

                                await OnWebSocketClosed.Invoke(new WebSocketClosedEventArgs {
                                    GuildId = guildId,
                                    Code = int.Parse($"{root.GetProperty("code")}"),
                                    Reason = $"{root.GetProperty("reason")}",
                                    ByRemote = bool.Parse($"{root.GetProperty("byRemote")}")
                                });
                                break;
                            }
                        }

                        break;
                    }

                    default: {
                        Log(LogSeverity.Error,
                            $"Unknown OP code received ({opElement}), check Lavalink implementation.");
                        break;
                    }
                }
            }
            catch (Exception exception) {
                if (exception is WebSocketClosedException) {
                    Volatile.Write(ref _refConnected, false);
                }

                Log(LogSeverity.Error, exception.Message, exception);
            }
        }

        private Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState pastState,
                                                  SocketVoiceState currentState) {
            if (_socketClient.CurrentUser?.Id != user.Id) {
                return Task.CompletedTask;
            }

            var voiceState = new VoiceState {
                UserId = user.Id,
                GuildId = (currentState.VoiceChannel ?? pastState.VoiceChannel).Guild.Id,
                SessionId = currentState.VoiceSessionId ?? pastState.VoiceSessionId,
                ChannelId = (currentState.VoiceChannel ?? pastState.VoiceChannel).Id
            };

            _voiceStates.AddOrUpdate(voiceState.GuildId, voiceState, (_, __) => voiceState);
            return Task.CompletedTask;
        }

        private async Task OnVoiceServerUpdatedAsync(SocketVoiceServer voiceServer) {
            if (!_voiceStates.TryGetValue(voiceServer.Guild.Id, out var voiceState)) {
                return;
            }

            var payload = new ServerUpdatePayload {
                SessionId = voiceState.SessionId,
                GuildId = $"{voiceServer.Guild.Id}",
                VoiceServerPayload = new VoiceServerPayload {
                    Endpoint = voiceServer.Endpoint,
                    Token = voiceServer.Token
                }
            };

            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        private void Log(LogSeverity severity, string message, Exception exception = null) {
            var logMessage = new LogMessage(severity, nameof(Victoria), message, exception);
            OnLog?.Invoke(logMessage)
                .ConfigureAwait(false);
        }
    }
}