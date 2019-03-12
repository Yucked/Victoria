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
        private SocketVoiceState cachedStated;
        protected Func<LogMessage, Task> _log;
        protected ConcurrentDictionary<ulong, LavaPlayer> _players;

        protected async Task InitializeAsync(BaseSocketClient baseSocketClient, Configuration configuration)
        {
            this.baseSocketClient = baseSocketClient;
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();
            baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;

            socketHelper = new SocketHelper(configuration, _log);
            socketHelper.OnMessage += OnMessage;

            await socketHelper.ConnectAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Connects to <paramref name="voiceChannel"/> and returns a <see cref="LavaPlayer"/>.
        /// </summary>
        /// <param name="voiceChannel">Voice channel to connect to.</param>
        /// <param name="textChannel">Optional text channel that can send updates.</param>
        public async Task<LavaPlayer> ConnectAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = null)
        {
            if (_players.TryGetValue(voiceChannel.GuildId, out var player))
                return player;


            player = new LavaPlayer(voiceChannel, textChannel, socketHelper);
            await voiceChannel.ConnectAsync(false, false, true).ConfigureAwait(false);
            _players.TryAdd(voiceChannel.GuildId, player);

            return player;
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
        /// <returns></returns>
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

        private bool OnMessage(string message)
        {
            _log?.WriteLog(LogSeverity.Verbose, message);
            var json = JObject.Parse(message);

            var guildId = (ulong)0;
            var player = default(LavaPlayer);

            if (json.TryGetValue("guildId", out var guildToken))
                guildId = ulong.Parse($"{guildToken}");

            var opCode = $"{json.GetValue("op")}";
            switch (opCode)
            {
                case "playerUpdate":
                    var state = json.GetValue("state");

                    if (!_players.TryGetValue(guildId, out player))
                        break;

                    var statePos = state["position"].ToObject<long>();
                    var stateTime = state["time"].ToObject<long>();

                    var position = TimeSpan.FromMilliseconds(statePos);

                    player.CurrentTrack.Position = position;
                    player.LastUpdate = DateTimeOffset.FromUnixTimeMilliseconds(stateTime);

                    _players.TryUpdate(guildId, player, player);
                    OnPlayerUpdated?.Invoke(player, player.CurrentTrack, position);
                    break;

                case "stats":
                    var stats = json.ToObject<ServerStats>();
                    ServerStats = stats;
                    OnServerStats?.Invoke(stats);
                    break;

                case "event":
                    var evt = json.GetValue("type").ToObject<EventType>();
                    _players.TryGetValue(guildId, out player);

                    LavaTrack track = default;
                    if (json.TryGetValue("track", out var hash))
                    {
                        track = TrackHelper.DecodeTrack($"{hash}");
                    }

                    switch (evt)
                    {
                        case EventType.TrackEnd:
                            var endReason = json.GetValue("reason").ToObject<TrackEndReason>();
                            if (endReason != TrackEndReason.Replaced)
                                player.CurrentTrack = default;
                            _players.TryUpdate(guildId, player, player);
                            OnTrackFinished?.Invoke(player, track, endReason);
                            break;

                        case EventType.TrackException:
                            var error = json.GetValue("error").ToObject<string>();
                            player.CurrentTrack = track;
                            _players.TryUpdate(guildId, player, player);
                            OnTrackException?.Invoke(player, track, error);
                            break;

                        case EventType.TrackStuck:
                            var timeout = json.GetValue("thresholdMs").ToObject<long>();
                            player.CurrentTrack = track;
                            _players.TryUpdate(guildId, player, player);
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

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState currentState)
        {
            if (user.Id != baseSocketClient.CurrentUser.Id)
                return;

            cachedStated = currentState;

            if (oldState.VoiceChannel != null && currentState.VoiceChannel is null)
            {
                if (!_players.TryGetValue(oldState.VoiceChannel.Id, out var player))
                    return;

                await player.DisposeAsync().ConfigureAwait(false);
                var destroy = new DestroyPayload(oldState.VoiceChannel.Guild.Id);
                await socketHelper.SendPayloadAsync(destroy).ConfigureAwait(false);
            }
        }

        private Task OnVoiceServerUpdated(SocketVoiceServer server)
        {
            if (!server.Guild.HasValue || !_players.TryGetValue(server.Guild.Id, out var player))
                return Task.CompletedTask;

            var update = new VoiceServerPayload(server, cachedStated.VoiceSessionId);
            return socketHelper.SendPayloadAsync(update);
        }
    }
}