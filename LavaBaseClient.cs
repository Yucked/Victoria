using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
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
        private Task disconnectTask;
        private CancellationTokenSource cancellationTokenSource;
        protected Configuration configuration;
        protected Func<LogMessage, Task> _log;
        protected ConcurrentDictionary<ulong, LavaPlayer> _players;

        protected async Task InitializeAsync(BaseSocketClient baseSocketClient, Configuration configuration)
        {
            configuration ??= new Configuration();
            
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
            cancellationTokenSource = new CancellationTokenSource();
            baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;

            socketHelper = new SocketHelper(configuration, _log);
            socketHelper.OnMessage += OnMessage;
            socketHelper.OnClosed += OnClosedAsync;

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

            await voiceChannel.ConnectAsync(configuration.SelfDeaf, false, true).ConfigureAwait(false);
            player = new LavaPlayer(voiceChannel, textChannel, socketHelper);
            _players.TryAdd(voiceChannel.GuildId, player);

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
        /// Moves voice channels and updates <see cref="LavaPlayer.VoiceChannel"/>.
        /// </summary>
        /// <param name="voiceChannel"><see cref="IVoiceChannel"/></param>
        public async Task MoveChannelsAsync(IVoiceChannel voiceChannel)
        {
            if (!_players.TryGetValue(voiceChannel.GuildId, out var player))
                return;

            if (player.VoiceChannel.Id == voiceChannel.Id)
                return;

            await player.PauseAsync();
            await player.VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            await voiceChannel.ConnectAsync(configuration.SelfDeaf, false, true).ConfigureAwait(false);
            await player.ResumeAsync();

            player.VoiceChannel = voiceChannel;
        }

        /// <summary>
        /// Update the <see cref="LavaPlayer.TextChannel"/>.
        /// </summary>
        /// <param name="channel"><see cref="ITextChannel"/></param>
        public void UpdateTextChannel(ulong guildId, ITextChannel textChannel)
        {
            if (!_players.TryGetValue(guildId, out var player))
                return;

            player.TextChannel = textChannel;
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

        public void ToggleAutoDisconnect()
        {
            configuration.AutoDisconnect = !configuration.AutoDisconnect;
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
            if (configuration.PreservePlayers)
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
                            if (endReason != TrackEndReason.Replaced)
                            {
                                player.IsPlaying = false;
                                player.CurrentTrack = default;
                            }
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

        private SocketVoiceChannel GetVoiceChannel(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var botUser = (oldState.VoiceChannel ?? newState.VoiceChannel).Guild.CurrentUser;
            if (oldState.VoiceChannel != botUser.VoiceChannel
                && newState.VoiceChannel != botUser.VoiceChannel) // unrelated channel activities
            {
                return null;
            }

            if (newState.VoiceChannel != null)
            {
                if (user.Id == baseSocketClient.CurrentUser.Id) // we moved of channel
                    return newState.VoiceChannel;

                if (botUser.VoiceChannel == newState.VoiceChannel) // a user joined our channel
                    return newState.VoiceChannel;
            }
            else
            {
                if (oldState.VoiceChannel != null && user.Id != baseSocketClient.CurrentUser.Id) // user disconnected
                    return oldState.VoiceChannel;
            }

            return null;
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var channel = GetVoiceChannel(user, oldState, newState);
            if (channel == null) return Task.CompletedTask;

            var guildId = channel.Guild.Id;
            if (_players.TryGetValue(guildId, out var player)
                && user.Id == baseSocketClient.CurrentUser.Id)
            {
                player.cachedState = newState;
            }

            if (configuration.AutoDisconnect)
            {
                var users = channel.Users.Count(x => !x.IsBot);

                if (users > 0)
                {
                    if (disconnectTask is null)
                        return Task.CompletedTask;

                    cancellationTokenSource.Cancel(false);
                    cancellationTokenSource = new CancellationTokenSource();
                    return Task.CompletedTask;
                }

                if (!(player is null))
                {
                    _log?.WriteLog(LogSeverity.Warning, $"Automatically disconnecting in {configuration.InactivityTimeout.TotalSeconds} seconds.");
                    disconnectTask = Task.Run(async () =>
                    {
                        await Task.Delay(configuration.InactivityTimeout).ConfigureAwait(false);
                        if (player.IsPlaying)
                            await player.StopAsync().ConfigureAwait(false);
                        await DisconnectAsync(player.VoiceChannel).ConfigureAwait(false);
                    }, cancellationTokenSource.Token);
                }
            }

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
