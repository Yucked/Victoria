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
using Microsoft.Extensions.Logging;
using Victoria.Node.EventArgs;
using Victoria.Payloads;
using Victoria.Payloads.Player;
using Victoria.Player;
using Victoria.Player.Decoder;
using Victoria.Responses;
using Victoria.Responses.Search;
using Victoria.WebSocket;
using Victoria.WebSocket.EventArgs;

// ReSharper disable SuggestBaseTypeForParameter

namespace Victoria.Node {
    /// <summary>
    /// Represents a single connection to a Lavalink server.
    /// </summary>
    public class LavaNode : LavaNode<LavaPlayer<LavaTrack>, LavaTrack> {
        /// <inheritdoc />
        public LavaNode(DiscordSocketClient socketClient,
                        NodeConfiguration nodeConfiguration,
                        ILogger<LavaNode> logger)
            : base(socketClient, nodeConfiguration, logger) { }

        /// <inheritdoc />
        public LavaNode(DiscordShardedClient shardedClient,
                        NodeConfiguration nodeConfiguration,
                        ILogger<LavaNode> logger)
            : base(shardedClient, nodeConfiguration, logger) { }
    }

    /// <summary>
    ///     Represents a single connection to a Lavalink server with custom <typeparamref name="TLavaPlayer"/>.
    /// </summary>
    /// <typeparam name="TLavaPlayer">Where TPlayer is inherited from <see cref="LavaPlayer" /></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    /// .
    public class LavaNode<TLavaPlayer, TLavaTrack> : IAsyncDisposable
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        /// <summary>
        ///     Checks if the client has an active WebSocket connection.
        /// </summary>
        public bool IsConnected
            => Volatile.Read(ref _refConnected);

        /// <summary>
        ///     Collection of <typeparamref name="TLavaPlayer" />.
        /// </summary>
        public IEnumerable<TLavaPlayer> Players
            => _playerCache.Values;

        /// <summary>
        /// 
        /// </summary>
        public event Func<StatsEventArg, Task> OnStatsReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Func<UpdateEventArg<TLavaPlayer, TLavaTrack>, Task> OnUpdateReceived;

        /// <summary>
        /// 
        /// </summary>
        public event Func<TrackStartEventArg<TLavaPlayer, TLavaTrack>, Task> OnTrackStart;

        /// <summary>
        /// 
        /// </summary>
        public event Func<TrackEndEventArg<TLavaPlayer, TLavaTrack>, Task> OnTrackEnd;

        /// <summary>
        /// 
        /// </summary>
        public event Func<TrackExceptionEventArg<TLavaPlayer, TLavaTrack>, Task> OnTrackException;

        /// <summary>
        /// 
        /// </summary>
        public event Func<TrackStuckEventArg<TLavaPlayer, TLavaTrack>, Task> OnTrackStuck;

        /// <summary>
        /// 
        /// </summary>
        public event Func<WebSocketClosedEventArg, Task> OnWebSocketClosed;

        private readonly ILogger<LavaNode<TLavaPlayer, TLavaTrack>> _logger;
        private readonly NodeConfiguration _nodeConfiguration;
        private readonly WebSocketClient _webSocketClient;
        private readonly BaseSocketClient _baseSocketClient;
        private readonly ConcurrentDictionary<ulong, TLavaPlayer> _playerCache;
        private readonly ConcurrentDictionary<ulong, VoiceState> _voiceStates;

        private bool _refConnected;

        /// <inheritdoc />
        public LavaNode(DiscordSocketClient socketClient,
                        NodeConfiguration nodeConfiguration,
                        ILogger<LavaNode<TLavaPlayer, TLavaTrack>> logger)
            : this(socketClient as BaseSocketClient, nodeConfiguration, logger) { }

        /// <inheritdoc />
        public LavaNode(DiscordShardedClient shardedClient,
                        NodeConfiguration nodeConfiguration,
                        ILogger<LavaNode<TLavaPlayer, TLavaTrack>> logger)
            : this(shardedClient as BaseSocketClient, nodeConfiguration, logger) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeConfiguration"></param>
        /// <param name="logger"></param>
        public LavaNode(NodeConfiguration nodeConfiguration,
                        ILogger<LavaNode<TLavaPlayer, TLavaTrack>> logger)
            : this(default(BaseSocketClient), nodeConfiguration, logger) { }

        private LavaNode(BaseSocketClient socketClient, NodeConfiguration nodeConfiguration,
                         ILogger<LavaNode<TLavaPlayer, TLavaTrack>> logger) {
            _nodeConfiguration = nodeConfiguration;
            _logger = logger;

            _baseSocketClient = socketClient;
            _baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
            _baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;

            nodeConfiguration.SocketConfiguration.Endpoint =
                (_nodeConfiguration.IsSecure ? "wss" : "ws") + _nodeConfiguration.Endpoint;
            _webSocketClient = new WebSocketClient(_nodeConfiguration.SocketConfiguration);
            _webSocketClient.OnOpenAsync += OnOpenAsync;
            _webSocketClient.OnErrorAsync += OnErrorAsync;
            _webSocketClient.OnCloseAsync += OnCloseAsync;
            _webSocketClient.OnDataAsync += OnDataAsync;
            _webSocketClient.OnRetryAsync += OnRetryAsync;

            _playerCache = new ConcurrentDictionary<ulong, TLavaPlayer>();
            _voiceStates = new ConcurrentDictionary<ulong, VoiceState>();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            await DisconnectAsync()
                .ConfigureAwait(false);

            await _webSocketClient.DisposeAsync()
                .ConfigureAwait(false);

            _playerCache.Clear();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Starts a WebSocket connection to the specified <see cref="NodeConfiguration.Hostname" />:<see cref="NodeConfiguration.Port" />
        ///     and hooks into <see cref="BaseSocketClient" /> events.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if client is already connected.</exception>
        public async Task ConnectAsync() {
            if (Volatile.Read(ref _refConnected)) {
                throw new InvalidOperationException(
                    $"You must call {nameof(DisconnectAsync)} or {nameof(DisposeAsync)} before calling {nameof(ConnectAsync)}.");
            }

            if (_baseSocketClient.CurrentUser == null || _baseSocketClient.CurrentUser.Id == 0) {
                throw new InvalidOperationException($"{nameof(_baseSocketClient)} is not in ready state.");
            }

            _webSocketClient.AddHeader("Authorization", _nodeConfiguration.Authorization);
            _webSocketClient.AddHeader("User-Id", $"{_baseSocketClient.CurrentUser.Id}");
            _webSocketClient.AddHeader("Client-Name",
                $"{nameof(Victoria)}/{typeof(LavaNode).Assembly.GetName().Version}");

            if (_nodeConfiguration.EnableResume) {
                _webSocketClient.AddHeader("Resume-Key", _nodeConfiguration.ResumeKey);
            }

            if (!string.IsNullOrWhiteSpace(_nodeConfiguration.UserAgent)) {
                _webSocketClient.AddHeader("User-Agent", _nodeConfiguration.UserAgent);
            }

            await _webSocketClient.ConnectAsync()
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

            await Parallel.ForEachAsync(_playerCache,
                async (cache, _) => {
                    _playerCache.TryRemove(cache.Key, out var player);
                    await LeaveAsync(player.VoiceChannel)
                        .ConfigureAwait(false);

                    await player.DisposeAsync()
                        .ConfigureAwait(false);
                });

            await _webSocketClient.DisconnectAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Joins the specified voice channel and returns the connected <typeparamref name="TLavaPlayer" />.
        /// </summary>
        /// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
        /// <param name="textChannel">An instance of <see cref="ITextChannel" />.</param>
        /// <returns>
        ///     <typeparamref name="TLavaPlayer" />
        /// </returns>
        public async Task<TLavaPlayer> JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = default) {
            if (!Volatile.Read(ref _refConnected)) {
                throw new InvalidOperationException(
                    $"You must call {nameof(ConnectAsync)} before joining a voice channel.");
            }

            if (voiceChannel == null) {
                throw new ArgumentNullException(nameof(voiceChannel));
            }

            if (_playerCache.TryGetValue(voiceChannel.GuildId, out var player)) {
                return player;
            }

            await voiceChannel.ConnectAsync(_nodeConfiguration.SelfDeaf, false, true)
                .ConfigureAwait(false);

            player = (TLavaPlayer)Activator
                .CreateInstance(typeof(TLavaPlayer), _webSocketClient, voiceChannel, textChannel);

            _playerCache.TryAdd(voiceChannel.GuildId, player);
            return player;
        }

        /// <summary>
        ///     Leaves the specified channel only if <typeparamref name="TLavaPlayer" /> is connected to it.
        /// </summary>
        /// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
        /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
        public async Task LeaveAsync(IVoiceChannel voiceChannel) {
            if (!Volatile.Read(ref _refConnected)) {
                throw new InvalidOperationException("Can't execute this operation when websocket isn't connected.");
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
        /// 
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<SearchResponse> SearchAsync(SearchType searchType, string query) {
            if (string.IsNullOrWhiteSpace(query)) {
                throw new ArgumentNullException(nameof(query));
            }

            var urlPath = searchType switch {
                SearchType.SoundCloud   => $"/loadtracks?identifier={WebUtility.UrlEncode($"scsearch:{query}")}",
                SearchType.YouTubeMusic => $"/loadtracks?identifier={WebUtility.UrlEncode($"ytmsearch:{query}")}",
                SearchType.YouTube      => $"/loadtracks?identifier={WebUtility.UrlEncode($"ytsearch:{query}")}",
                SearchType.Direct or _  => $"/loadtracks?identifier={query}"
            };

            using var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, $"{_nodeConfiguration.HttpEndpoint}{urlPath}") {
                    Headers = {
                        { "Authorization", _nodeConfiguration.Authorization }
                    }
                };

            var searchResponse = await VictoriaExtensions.ReadAsJsonAsync<SearchResponse>(requestMessage);
            return searchResponse;
        }

        /// <summary>
        ///     Checks if <typeparamref name="TLavaPlayer" /> exists for specified guild.
        /// </summary>
        /// <param name="guild">An instance of <see cref="IGuild" />.</param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool HasPlayer(IGuild guild) {
            return _playerCache.ContainsKey(guild.Id);
        }

        /// <summary>
        ///     Returns either an existing or null player.
        /// </summary>
        /// <param name="guild">An instance of <see cref="IGuild" />.</param>
        /// <param name="player">An instance of <typeparamref name="TLavaPlayer" /></param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool TryGetPlayer(IGuild guild, out TLavaPlayer player) {
            return _playerCache.TryGetValue(guild.Id, out player);
        }

        private Task OnOpenAsync() {
            Volatile.Write(ref _refConnected, true);
            if (!_nodeConfiguration.EnableResume) {
                return Task.CompletedTask;
            }

            return _webSocketClient.SendAsync(
                new ResumePayload(_nodeConfiguration.ResumeKey, _nodeConfiguration.ResumeTimeout));
        }

        private Task OnCloseAsync(CloseEventArgs arg) {
            Volatile.Write(ref _refConnected, false);
            _logger.LogWarning("WebSocket connection closed");
            return Task.CompletedTask;
        }

        private Task OnErrorAsync(ErrorEventArgs arg) {
            _logger.LogError(arg.Exception, arg.Message);
            return Task.CompletedTask;
        }

        private Task OnRetryAsync(RetryEventArgs arg) {
            if (arg.IsLastRetry) {
                _logger.LogError("This was the last try in establishing connection with Lavalink");
                return Task.CompletedTask;
            }

            _logger.LogWarning($"Lavalink reconnect attempt #{arg.Count}");
            return Task.CompletedTask;
        }

        private async Task OnDataAsync(DataEventArgs arg) {
            if (arg.IsEmpty) {
                _logger.LogWarning("Didn't receive any data from websocket");
                return;
            }

            _logger.LogDebug(arg.Data);
            using var document = JsonDocument.Parse(arg.Data);
            var root = document.RootElement;

            try {
                switch (VictoriaExtensions.GetOp(root)) {
                    case "stats":
                        if (OnStatsReceived == null) {
                            return;
                        }

                        await OnStatsReceived.Invoke(JsonSerializer.Deserialize<StatsEventArg>(arg.Data));
                        break;

                    case "playerUpdate":
                        var (guildId, time, position, isConnected) = VictoriaExtensions.GetPlayerUpdate(root);
                        if (!_playerCache.TryGetValue(guildId, out var player)) {
                            return;
                        }

                        player?.Track?.UpdatePosition(position);
                        player.LastUpdate = DateTimeOffset.FromUnixTimeMilliseconds(time);
                        player.IsConnected = isConnected;

                        if (OnUpdateReceived == null) {
                            return;
                        }

                        await OnUpdateReceived.Invoke(new UpdateEventArg<TLavaPlayer, TLavaTrack> {
                            Player = player,
                            Track = player.Track,
                            Position = player.Track?.Position ?? default
                        });
                        break;

                    case "event": {
                        guildId = ulong.Parse($"{root.GetProperty("guildId")}");
                        if (!_playerCache.TryGetValue(guildId, out player)) {
                            return;
                        }

                        var lavaTrack = default(TLavaTrack);
                        if (root.TryGetProperty("track", out var trackElement)) {
                            lavaTrack = TrackDecoder.Decode($"{trackElement}", player.Track);
                        }

                        var type = $"{root.GetProperty("type")}";
                        switch (type) {
                            case "TrackStartEvent":
                                player.PlayerState = PlayerState.Playing;

                                if (OnTrackStart == null) {
                                    break;
                                }

                                await OnTrackStart.Invoke(new TrackStartEventArg<TLavaPlayer, TLavaTrack> {
                                    Player = player,
                                    Track = lavaTrack
                                });
                                break;

                            case "TrackEndEvent":
                                var trackEndReason = (TrackEndReason)(byte)$"{root.GetProperty("reason")}"[0];
                                if (trackEndReason is not TrackEndReason.Replaced) {
                                    player.Track = default;
                                    player.PlayerState = PlayerState.Stopped;
                                }

                                if (OnTrackEnd == null) {
                                    break;
                                }

                                await OnTrackEnd.Invoke(new TrackEndEventArg<TLavaPlayer, TLavaTrack> {
                                    Player = player,
                                    Track = lavaTrack,
                                    Reason = trackEndReason
                                });
                                break;

                            case "TrackExceptionEvent":
                                player.Track = default;
                                player.PlayerState = PlayerState.Stopped;

                                if (OnTrackException == null) {
                                    break;
                                }

                                await OnTrackException.Invoke(new TrackExceptionEventArg<TLavaPlayer, TLavaTrack> {
                                    Player = player,
                                    Track = lavaTrack,
                                    Exception = new LavaException {
                                        Message = root.GetProperty("message").GetString(),
                                        Severity = root.GetProperty("severity").GetString(),
                                    }
                                });
                                break;

                            case "TrackStuckEvent":
                                player.Track = default;
                                player.PlayerState = PlayerState.Stopped;

                                if (OnTrackStuck == null) {
                                    break;
                                }

                                await OnTrackStuck.Invoke(new TrackStuckEventArg<TLavaPlayer, TLavaTrack> {
                                    Player = player,
                                    Track = lavaTrack,
                                    Threshold = long.Parse($"{root.GetProperty("thresholdMs")}")
                                });
                                break;

                            case "WebSocketClosedEvent":
                                if (OnWebSocketClosed == null) {
                                    break;
                                }

                                await OnWebSocketClosed.Invoke(new WebSocketClosedEventArg {
                                    Guild = _baseSocketClient.GetGuild(guildId),
                                    Code = int.Parse($"{root.GetProperty("code")}"),
                                    Reason = $"{root.GetProperty("reason")}",
                                    ByRemote = bool.Parse($"{root.GetProperty("byRemote")}")
                                });
                                break;

                            default:
                                _logger.LogWarning(
                                    "Unknown event type received: {type}", type);
                                break;
                        }
                    }
                        break;

                    default:
                        _logger.LogWarning("Unknown OP code received, check Lavalink implementation");
                        break;
                }
            }
            catch (Exception exception) {
                _logger.LogError(exception.Message, exception);
            }
        }

        private Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState pastState,
                                                  SocketVoiceState currentState) {
            if (_baseSocketClient.CurrentUser?.Id != user.Id) {
                return Task.CompletedTask;
            }

            var voiceState = new VoiceState {
                UserId = user.Id,
                GuildId = (currentState.VoiceChannel ?? pastState.VoiceChannel).Guild.Id,
                SessionId = currentState.VoiceSessionId ?? pastState.VoiceSessionId,
                ChannelId = (currentState.VoiceChannel ?? pastState.VoiceChannel).Id
            };

            _voiceStates.AddOrUpdate(voiceState.GuildId, voiceState, (_, _) => voiceState);
            return Task.CompletedTask;
        }

        private Task OnVoiceServerUpdatedAsync(SocketVoiceServer voiceServer) {
            if (!_voiceStates.TryGetValue(voiceServer.Guild.Id, out var voiceState)) {
                return Task.CompletedTask;
            }

            return _webSocketClient.SendAsync(new ServerUpdatePayload {
                SessionId = voiceState.SessionId,
                GuildId = $"{voiceServer.Guild.Id}",
                VoiceServerPayload = new VoiceServerPayload {
                    Endpoint = voiceServer.Endpoint,
                    Token = voiceServer.Token
                }
            });
        }
    }
}
