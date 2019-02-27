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
    public abstract class BaseLavaClient
    {
        /// <summary>
        /// 
        /// </summary>
        public event Func<LogMessage, Task> Log;

        /// <summary>
        /// 
        /// </summary>
        public event Func<ServerStats, Task> OnServerStats;

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

        private SocketVoiceState cachedStated;
        private readonly BaseSocketClient _baseSocketClient;
        private readonly SocketHelper _socketHelper;
        private readonly LogSeverity _logSeverity;
        protected ConcurrentDictionary<ulong, LavaPlayer> _players;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseSocketClient"></param>
        /// <param name="configuration"></param>
        protected BaseLavaClient(BaseSocketClient baseSocketClient, Configuration configuration)
        {
            if (!(baseSocketClient is DiscordSocketClient) || !(baseSocketClient is DiscordShardedClient))
                throw new ArgumentException();

            _baseSocketClient = baseSocketClient;
            configuration.UserId = baseSocketClient.CurrentUser.Id;
            _logSeverity = configuration.LogSeverity.Value;
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();
            baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;

            _socketHelper = new SocketHelper(configuration, ref Log);
            _socketHelper.OnMessage += OnMessage;
        }

        public async Task StartAsync()
        {
            await _socketHelper.ConnectAsync().ConfigureAwait(false);
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


            player = new LavaPlayer(voiceChannel, textChannel, _socketHelper);
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
            GC.SuppressFinalize(this);
        }

        #region PRIVATES
        private bool OnMessage(string message)
        {
            Console.WriteLine(message);
            var json = JObject.Parse(message);
            var guildId = (ulong)0;

            if (json.TryGetValue("guildId", out var guildToken))
                guildId = ulong.Parse($"{guildToken}");

            var opCode = $"{json.GetValue("op")}";
            switch (opCode)
            {
                case "playerUpdate":

                    break;

                case "stats":
                    var stats = json.ToObject<ServerStats>();
                    ServerStats = stats;
                    OnServerStats?.Invoke(stats);
                    break;

                case "event":
                    var evt = json.GetValue("type").ToObject<EventType>();
                    _players.TryGetValue(guildId, out var player);

                    switch (evt)
                    {
                        case EventType.TrackEnd:
                            var endReason = json.GetValue("reason").ToObject<TrackEndReason>();
                            break;

                        case EventType.TrackException:
                            break;

                        case EventType.TrackStuck:
                            break;

                        case EventType.WebSocketClosed:
                            break;

                        default:
                            break;
                    }

                    break;

                default:
                    break;
            }

            return true;
        }

        private void WriteLog(LogSeverity severity, string message, Exception exception = null)
        {
            if (severity >= _logSeverity)
                return;

            Log?.Invoke(VictoriaExtensions.LogMessage(severity, message, exception));
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState currentState)
        {
            if (user.Id != _baseSocketClient.CurrentUser.Id)
                return;

            cachedStated = currentState;

            if (oldState.VoiceChannel != null && currentState.VoiceChannel is null)
            {
                if (!_players.TryGetValue(oldState.VoiceChannel.Id, out var player))
                    return;

                await player.DisposeAsync().ConfigureAwait(false);
                var destroy = new DestroyPayload(oldState.VoiceChannel.Guild.Id);
                await _socketHelper.SendPayloadAsync(destroy).ConfigureAwait(false);
            }
        }

        private Task OnVoiceServerUpdated(SocketVoiceServer server)
        {
            if (!server.Guild.HasValue || !_players.TryGetValue(server.Guild.Id, out var player))
                return Task.CompletedTask;

            var update = new VoiceServerPayload(server, cachedStated.VoiceSessionId);
            return _socketHelper.SendPayloadAsync(update);
        }

        #endregion
    }
}