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
            add { ShadowLog += value; }
            remove { ShadowLog -= value; }
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

        private BaseSocketClient _baseSocketClient;
        private SocketHelper _socketHelper;
        private Task _disconnectTask;
        private CancellationTokenSource _cancellationTokenSource;
        protected Configuration Configuration;
        protected Func<LogMessage, Task> ShadowLog;
        protected ConcurrentDictionary<ulong, LavaPlayer> Players;

        protected async Task InitializeAsync(BaseSocketClient baseSocketClient, Configuration configuration)
        {
            configuration ??= new Configuration();

            _baseSocketClient = baseSocketClient;
            var shards = baseSocketClient switch 
            { 
                DiscordSocketClient socketClient => await socketClient.GetRecommendedShardCountAsync(), 
                DiscordShardedClient shardedClient => shardedClient.Shards.Count, _ => 1 
            };

            Configuration = configuration.SetInternals(baseSocketClient.CurrentUser.Id, shards);
            Players = new ConcurrentDictionary<ulong, LavaPlayer>();
            _cancellationTokenSource = new CancellationTokenSource();
            baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;

            _socketHelper = new SocketHelper(configuration, ShadowLog);
            _socketHelper.OnMessage += OnMessage;
            _socketHelper.OnClosed += OnClosedAsync;

            await _socketHelper.ConnectAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Connects to <paramref name="voiceChannel"/> and returns a <see cref="LavaPlayer"/>.
        /// </summary>
        /// <param name="voiceChannel">Voice channel to connect to.</param>
        /// <param name="textChannel">Optional text channel that can send updates.</param>
        public async Task<LavaPlayer> ConnectAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = null)
        {
            if (Players.TryGetValue(voiceChannel.GuildId, out var player))
                return player;

            await voiceChannel.ConnectAsync(Configuration.SelfDeaf, false, true).ConfigureAwait(false);
            player = new LavaPlayer(voiceChannel, textChannel, _socketHelper);
            Players.TryAdd(voiceChannel.GuildId, player);
            if (Configuration.DefaultVolume != 100)
                await player.SetVolumeAsync(Configuration.DefaultVolume);

            return player;
        }

        /// <summary>
        /// Disconnects from the <paramref name="voiceChannel"/>.
        /// </summary>
        /// <param name="voiceChannel">Connected voice channel.</param>
        public async Task DisconnectAsync(IVoiceChannel voiceChannel)
        {
            if (!Players.TryRemove(voiceChannel.GuildId, out _))
                return;

            await voiceChannel.DisconnectAsync().ConfigureAwait(false);
            var destroyPayload = new DestroyPayload(voiceChannel.GuildId);
            await _socketHelper.SendPayloadAsync(destroyPayload);
        }

        /// <summary>
        /// Moves voice channels and updates <see cref="LavaPlayer.VoiceChannel"/>.
        /// </summary>
        /// <param name="voiceChannel"><see cref="IVoiceChannel"/></param>
        public async Task MoveChannelsAsync(IVoiceChannel voiceChannel)
        {
            if (!Players.TryGetValue(voiceChannel.GuildId, out var player))
                return;

            if (player.VoiceChannel.Id == voiceChannel.Id)
                return;

            await player.PauseAsync();
            await player.VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            await voiceChannel.ConnectAsync(Configuration.SelfDeaf, false, true).ConfigureAwait(false);
            await player.ResumeAsync();

            player.VoiceChannel = voiceChannel;
        }

        /// <summary>
        /// Update the <see cref="LavaPlayer.TextChannel"/>.
        /// </summary>
        /// <param name="guildId">Guild Id</param>
        /// <param name="textChannel"><see cref="ITextChannel"/></param>
        public void UpdateTextChannel(ulong guildId, ITextChannel textChannel)
        {
            if (!Players.TryGetValue(guildId, out var player))
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
            return Players.TryGetValue(guildId, out var player) ? player : default;
        }

        /// <summary>
        /// Enables or disables AutoDisconnect <see cref="Configuration.AutoDisconnect"/>
        /// </summary>
        public void ToggleAutoDisconnect()
        {
            Configuration.AutoDisconnect = !Configuration.AutoDisconnect;
        }

        /// <summary>
        /// Disposes all <see cref="LavaPlayer"/>s and closes websocket connection.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            foreach (var player in Players.Values)
            {
                await player.DisposeAsync().ConfigureAwait(false);
            }

            Players.Clear();
            Players = null;
            await _socketHelper.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        private async Task OnClosedAsync()
        {
            if (Configuration.PreservePlayers)
                return;

            foreach (var player in Players.Values)
            {
                await DisconnectAsync(player.VoiceChannel).ContinueWith(_ => player.DisposeAsync());
            }

            Players.Clear();
            ShadowLog?.WriteLog(LogSeverity.Warning, "Lavalink died. Disposed all players.");
        }

        private bool OnMessage(string message)
        {
            ShadowLog?.WriteLog(LogSeverity.Debug, message);
            var json = JObject.Parse(message);

            var guildId = (ulong) 0;
            LavaPlayer player;

            if (json.TryGetValue("guildId", out var guildToken))
                guildId = ulong.Parse($"{guildToken}");

            var opCode = $"{json.GetValue("op")}";
            switch (opCode)
            {
                case "playerUpdate":
                    if (!Players.TryGetValue(guildId, out player))
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
                    if (!Players.TryGetValue(guildId, out player))
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
                            ShadowLog?.WriteLog(LogSeverity.Warning, $"Missing implementation of {evt} event.");
                            break;
                    }

                    break;

                default:
                    ShadowLog?.WriteLog(LogSeverity.Warning, $"Missing handling of {opCode} OP code.");
                    break;
            }

            return true;
        }

        private SocketVoiceChannel GetVoiceChannel(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var channel = oldState.VoiceChannel ?? newState.VoiceChannel;
            var botUser = channel.Guild.CurrentUser;

            if (oldState.VoiceChannel != botUser.VoiceChannel && newState.VoiceChannel != botUser.VoiceChannel)
                return null;

            switch (newState.VoiceChannel)
            {
                case null:
                    if (oldState.VoiceChannel != null && user.Id != _baseSocketClient.CurrentUser.Id)
                        return oldState.VoiceChannel;
                    break;

                default:
                    if (user.Id == _baseSocketClient.CurrentUser.Id)
                        return newState.VoiceChannel;

                    if (botUser.VoiceChannel == newState.VoiceChannel)
                        return newState.VoiceChannel;
                    break;
            }

            return channel;
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var channel = GetVoiceChannel(user, oldState, newState);
            if (channel == null)
                return Task.CompletedTask;

            var guildId = channel.Guild.Id;
            if (Players.TryGetValue(guildId, out var player) && user.Id == _baseSocketClient.CurrentUser.Id)
            {
                player.CachedState = newState;
            }

            if (!Configuration.AutoDisconnect)
                return Task.CompletedTask;
            
            var users = channel.Users.Count(x => !x.IsBot);

            if (users > 0)
            {
                if (_disconnectTask is null)
                    return Task.CompletedTask;

                _cancellationTokenSource.Cancel(false);
                _cancellationTokenSource = new CancellationTokenSource();
                return Task.CompletedTask;
            }

            if (player is null)
                return Task.CompletedTask;

            ShadowLog?.WriteLog(LogSeverity.Warning,
                                $"Automatically disconnecting in {Configuration.InactivityTimeout.TotalSeconds} seconds.");
            
            _disconnectTask = DisconnectTaskAsync(player, _cancellationTokenSource.Token);
            return Task.CompletedTask;
        }
        
        private async Task DisconnectTaskAsync(LavaPlayer player, CancellationToken token)
        {
            await Task.Delay(Configuration.InactivityTimeout, token).ConfigureAwait(false);
            
            if (token.IsCancellationRequested)
                return;
            
            if (player.IsPlaying)
                await player.StopAsync().ConfigureAwait(false);
            
            await DisconnectAsync(player.VoiceChannel).ConfigureAwait(false);
        }
        
        private Task OnVoiceServerUpdated(SocketVoiceServer server)
        {
            if (!server.Guild.HasValue || !Players.TryGetValue(server.Guild.Id, out var player))
                return Task.CompletedTask;

            var update = new VoiceServerPayload(server, player.CachedState.VoiceSessionId);
            return _socketHelper.SendPayloadAsync(update);
        }
    }
}