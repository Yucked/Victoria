using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Entities.Enums;
using Victoria.Entities.Payloads;
using Victoria.Entities.Responses;
using Victoria.Helpers;

namespace Victoria
{
    public abstract class LavaBaseClient
    {
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public event Func<ServerStats, Task> OnServerStats;

        /// <summary>
        /// 
        /// </summary>
        public event Func<int, string, bool, Task> OnSocketClosed;

        /// <summary>
        /// 
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, long, Task> OnTrackStuck;

        /// <summary>
        /// 
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, string, Task> OnTrackException;

        /// <summary>
        /// 
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, TimeSpan, Task> OnPlayerUpdated;

        /// <summary>
        /// 
        /// </summary>
        public event Func<LavaPlayer, LavaTrack, TrackEndReason, Task> OnTrackFinished;

        /// <summary>
        /// 
        /// </summary>
        public ServerStats ServerStats { get; private set; }

        private BaseSocketClient baseSocketClient;
        private LogSeverity logSeverity;
        private SocketHelper socketHelper;
        private SocketVoiceState cachedStated;
        protected Func<LogMessage, Task> _log;
        protected ConcurrentDictionary<ulong, LavaPlayer> _players;

        protected async Task InitializeAsync(BaseSocketClient baseSocketClient, Configuration configuration)
        {
            this.baseSocketClient = baseSocketClient;
            logSeverity = configuration.LogSeverity.Value;
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();
            baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;

            socketHelper = new SocketHelper(configuration, _log);
            socketHelper.OnMessage += OnMessage;

            await socketHelper.ConnectAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="voiceChannel"></param>
        /// <param name="textChannel"></param>
        /// <returns></returns>
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
        /// 
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

        #region PRIVATES
        private bool OnMessage(string message)
        {
            WriteLog(LogSeverity.Verbose, message);
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

                    var position = TimeSpan.FromMilliseconds(json.GetValue("position").ToObject<long>());
                    var time = json.GetValue("time").ToObject<long>();

                    player.CurrentTrack.Position = position;
                    player.LastUpdate = new DateTimeOffset(time * TimeSpan.TicksPerMillisecond + 621_355_968_000_000_000, TimeSpan.Zero);

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
                            WriteLog(LogSeverity.Warning, $"Missing implementation of {evt} event.");
                            break;
                    }

                    break;

                default:
                    WriteLog(LogSeverity.Warning, $"Missing handling of {opCode} OP code.");
                    break;
            }

            return true;
        }

        private void WriteLog(LogSeverity severity, string message, Exception exception = null)
        {
            if (severity >= logSeverity)
                return;

            _log?.Invoke(VictoriaExtensions.LogMessage(severity, message, exception));
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

        #endregion
    }
}