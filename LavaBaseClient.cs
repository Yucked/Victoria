using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Entities.Payloads;
using Victoria.Helpers;

namespace Victoria
{
    public abstract class LavaBaseClient
    {
        /// <summary>
        /// Spits out important information.
        /// </summary>
        public event Func<LogMessage, Task> Log
        {
            add
            {
                _log += value;
            }
            remove
            {
                _log -= value;
            }
        }

        /// <summary>
        /// Fires when Lavalink server sends stats.
        /// </summary>
        public event Func<ServerStats, Task> OnServerStats;

        /// <summary>
        /// Fires when Lavalink server closes connection. 
        /// Params are: <see cref="int"/> ErrorCode, <see cref="string"/> Reason, <see cref="bool"/> ByRemote.
        /// </summary>
        public event Func<int, string, bool, Task> OnSocketClosed;

        /// <summary>
        /// Fires when a <see cref="LavaTrack"/> is stuck. <see cref="long"/> specifies threshold.
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, long, Task> OnTrackStuck;

        /// <summary>
        /// Fires when <see cref="LavaTrack"/> throws an exception. <see cref="string"/> is the error reason.
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, string, Task> OnTrackException;

        /// <summary>
        /// Fires when <see cref="LavaTrack"/> receives an updated.
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, TimeSpan, Task> OnPlayerUpdated;

        /// <summary>
        /// Fires when a track has finished playing.
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, TrackEndReason, Task> OnTrackFinished;

        /// <summary>
        /// Keeps up to date with <see cref="OnServerStats"/>.
        /// </summary>
        public ServerStats ServerStats { get; private set; }

        private BaseSocketClient baseSocketClient;
        private SocketHelper socketHelper;
        private Configuration configuration;
        protected Func<LogMessage, Task> _log;
        protected ConcurrentDictionary<ulong, LavaPlayer> _players;

        protected async Task InitializeAsync(BaseSocketClient baseSocketClient, Configuration configuration)
        {
            this.baseSocketClient = baseSocketClient;
            var shards = baseSocketClient switch
            {
                DiscordSocketClient socketClient
                    => await socketClient.GetRecommendedShardCountAsync(),

                DiscordShardedClient shardedClient
                    => shardedClient.Shards.Count,

                _ => 1
            };

            this.configuration = configuration.SetInternals(baseSocketClient.CurrentUser.Id, shards);
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();
            baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;

            await InitializeWebSocketAsync().ConfigureAwait(false);
        }

        private Task InitializeWebSocketAsync()
        {
            socketHelper = new SocketHelper(this.configuration, _log);
            socketHelper.OnMessage += OnMessage;
            socketHelper.OnClosed += OnClosedAsync;

            return socketHelper.ConnectAsync();
        }

        /// <summary>
        /// Connects to <paramref name="voiceChannel"/> and returns a <see cref="LavaPlayer"/>.
        /// </summary>
        /// <param name="voiceChannel">Voice channel to connect to.</param>
        /// <param name="textChannel">Optional text channel that can send updates.</param>
        /// <param name="existing">If player already exists in cache. Works with <see cref="Configuration.ShouldPreservePlayers"/>.</param>
        public async Task<LavaPlayer> ConnectAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = null, bool existing = false)
        {
            if (_players.TryGetValue(voiceChannel.GuildId, out var player) && !existing)
                return player;

            await voiceChannel.ConnectAsync(configuration.SelfDeaf, false, true).ConfigureAwait(false);

            if (existing)
            {
                await InitializeWebSocketAsync().ConfigureAwait(false);
                player = new LavaPlayer(voiceChannel, textChannel, socketHelper);
                _players.TryUpdate(voiceChannel.GuildId, player, default);
            }
            else
            {
                player = new LavaPlayer(voiceChannel, textChannel, socketHelper);
                _players.TryAdd(voiceChannel.GuildId, player);
            }

            return player;
        }

        /// <summary>
        /// Disconnects from the <paramref name="voiceChannel"/>.
        /// </summary>
        /// <param name="voiceChannel">Connected voice channel.</param>
        public async Task DisconnectAsync(IVoiceChannel voiceChannel)
        {
            if (!_players.TryRemove(voiceChannel.GuildId, out _))
                return;

            await voiceChannel.DisconnectAsync().ConfigureAwait(false);
            var destroyPayload = new DestroyPayload(voiceChannel.GuildId);
            await socketHelper.SendPayloadAsync(destroyPayload);
        }

        /// <summary>
        /// Gets an existing <see cref="LavaPlayer"/> otherwise null.
        /// </summary>
        /// <param name="guildId">Id of the guild.</param>
        /// <returns><see cref="LavaPlayer"/></returns>
        public LavaPlayer GetPlayer(ulong guildId)
        {
            return _players.TryGetValue(guildId, out var player)
                ? player : default;
        }

        /// <summary>
        /// Disposes all <see cref="LavaPlayer"/>s and closes websocket connection.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            foreach (var player in _players.Values)
            {
                await player.DisposeAsync().ConfigureAwait(false);
            }
            _players.Clear();
            _players = null;
            await socketHelper.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        private async Task OnClosedAsync()
        {
            if (configuration.ShouldPreservePlayers)
                return;

            foreach (var player in _players.Values)
            {
                await DisconnectAsync(player.VoiceChannel).
                    ContinueWith(_ => player.DisposeAsync());
            }

            _players.Clear();
            _log?.WriteLog(LogSeverity.Warning, "Lavalink died. Disposed all players.");
        }

        private bool OnMessage(string message)
        {
            _log?.WriteLog(LogSeverity.Debug, message);
            var json = JObject.Parse(message);

            var guildId = (ulong)0;
            var player = default(LavaPlayer);

            if (json.TryGetValue("guildId", out var guildToken))
                guildId = ulong.Parse($"{guildToken}");

            var opCode = $"{json.GetValue("op")}";
            switch (opCode)
            {
                case "playerUpdate":
                    if (!_players.TryGetValue(guildId, out player))
                        return false;

                    var state = json.GetValue("state").ToObject<PlayerState>();
                    player.CurrentTrack.Position = state.Position;
                    player.LastUpdate = state.Time;

                    OnPlayerUpdated?.Invoke(player, player.CurrentTrack, state.Position);
                    break;

                case "stats":
                    ServerStats = json.ToObject<ServerStats>();
                    OnServerStats?.Invoke(ServerStats);
                    break;

                case "event":
                    var evt = json.GetValue("type").ToObject<EventType>();
                    if (!_players.TryGetValue(guildId, out player))
                        return false;

                    var track = default(LavaTrack);
                    if (json.TryGetValue("track", out var hash))
                        track = TrackHelper.DecodeTrack($"{hash}");

                    switch (evt)
                    {
                        case EventType.TrackEnd:
                            var endReason = json.GetValue("reason").ToObject<TrackEndReason>();
                            player.IsPlaying = false;
                            if (endReason != TrackEndReason.Replaced)
                                player.CurrentTrack = default;
                            OnTrackFinished?.Invoke(player, track, endReason);
                            break;

                        case EventType.TrackException:
                            var error = json.GetValue("error").ToObject<string>();
                            player.CurrentTrack = track;
                            OnTrackException?.Invoke(player, track, error);
                            break;

                        case EventType.TrackStuck:
                            var timeout = json.GetValue("thresholdMs").ToObject<long>();
                            player.CurrentTrack = track;
                            OnTrackStuck?.Invoke(player, track, timeout);
                            break;

                        case EventType.WebSocketClosed:
                            var reason = json.GetValue("reason").ToObject<string>();
                            var code = json.GetValue("code").ToObject<int>();
                            var byRemote = json.GetValue("byRemote").ToObject<bool>();
                            OnSocketClosed?.Invoke(code, reason, byRemote);
                            break;

                        default:
                            _log?.WriteLog(LogSeverity.Warning, $"Missing implementation of {evt} event.");
                            break;
                    }
                    break;

                default:
                    _log?.WriteLog(LogSeverity.Warning, $"Missing handling of {opCode} OP code.");
                    break;
            }

            return true;
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (user.Id != baseSocketClient.CurrentUser.Id)
                return Task.CompletedTask;

            var guildId = (oldState.VoiceChannel ?? newState.VoiceChannel).Guild.Id;

            if (!_players.TryGetValue(guildId, out var player))
                return Task.CompletedTask;

            player.cachedState = newState;

            return Task.CompletedTask;
        }

        private Task OnVoiceServerUpdated(SocketVoiceServer server)
        {
            if (!server.Guild.HasValue || !_players.TryGetValue(server.Guild.Id, out var player))
                return Task.CompletedTask;

            var update = new VoiceServerPayload(server, player.cachedState.VoiceSessionId);
            return socketHelper.SendPayloadAsync(update);
        }
    }
}