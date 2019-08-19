using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Socks.EventArgs;
using Victoria.Common;
using Victoria.Frostbyte.Enums;
using Victoria.Frostbyte.EventArgs;
using Victoria.Frostbyte.Infos;
using Victoria.Frostbyte.Payloads;
using Victoria.Frostbyte.Responses;

namespace Victoria.Frostbyte
{
    /// <summary>
    /// 
    /// </summary>
    public class FrostClient : BaseClient<FrostPlayer, TrackInfo>
    {
        /// <summary>
        ///     Fires whenever a log message is sent.
        /// </summary>
        public event Func<LogMessage, Task> OnLog;

        /// <summary>
        ///     Fires when metrics are received.
        /// </summary>
        public event Func<MetricsEventArgs, Task> OnMetricsReceived;

        /// <summary>
        ///     Fires when a track has finished playing.
        /// </summary>
        public event Func<TrackEndedEventArgs, Task> OnTrackEnded;

        /// <summary>
        ///     Fires when a track threw an exception.
        /// </summary>
        public event Func<TrackErrorEventArgs, Task> OnTrackError;

        /// <summary>
        ///     Fires when a track update is received.
        /// </summary>
        public event Func<TrackUpdateEventArgs, Task> OnTrackUpdate;

        private readonly FrostConfig _config;

        /// <inheritdoc />
        public FrostClient(DiscordSocketClient socketClient, FrostConfig config) : base(socketClient, config)
        {
            _config = config;
            socketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
            socketClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;
            Sock.OnReceive += OnReceiveAsync;
        }

        /// <inheritdoc />
        public FrostClient(DiscordShardedClient shardedClient, FrostConfig config) : base(shardedClient, config)
        {
            _config = config;
            shardedClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
            shardedClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;
            Sock.OnReceive += OnReceiveAsync;
        }

        /// <inheritdoc />
        public override Task ConnectAsync()
        {
            Sock.AddHeader("User-Id", $"{SocketClient.CurrentUser.Id}");
            Sock.AddHeader("Authorization", _config.Authorization);
            return base.ConnectAsync();
        }

        /// <inheritdoc />
        public override async Task<FrostPlayer> JoinAsync(IVoiceChannel voiceChannel,
            ITextChannel textChannel = default)
        {
            Ensure.NotNull(voiceChannel);

            if (PlayerCache.TryGetValue(voiceChannel.GuildId, out var player))
                return player;

            await voiceChannel.ConnectAsync(_config.SelfDeaf, false, true)
                .ConfigureAwait(false);

            player = new FrostPlayer(Sock, voiceChannel, textChannel);
            PlayerCache.TryAdd(voiceChannel.GuildId, player);
            return player;
        }
        
        private async Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldState,
            SocketVoiceState newState)
        {
            if (user.Id != SocketClient.CurrentUser.Id)
                return;

            var guildId = newState.VoiceChannel?.Guild.Id;

            if (!PlayerCache.TryGetValue(guildId.GetValueOrDefault(), out var player))
                return;

            player.VoiceState = newState;

            await Task.Delay(0)
                .ConfigureAwait(false);
        }

        private async Task OnVoiceServerUpdatedAsync(SocketVoiceServer voiceServer)
        {
            if (!PlayerCache.TryGetValue(voiceServer.Guild.Id, out var player))
                return;

            var payload = new VoiceServerPayload(voiceServer.Guild.Id)
            {
                Endpoint = voiceServer.Endpoint,
                Token = voiceServer.Token,
                SessionId = player.VoiceState.VoiceSessionId
            };

            await Sock.SendAsync(payload)
                .ConfigureAwait(false);
        }

        private async Task OnReceiveAsync(ReceivedEventArgs eventArgs)
        {
            if (eventArgs.DataSize < 1)
            {
                Log(LogSeverity.Warning, nameof(Frostbyte), "Received an empty payload from Frostbyte.");
                return;
            }

            Log(LogSeverity.Warning, nameof(Frostbyte), eventArgs.Raw);
            
            var response = Json.Deserialize<EventResponse>(eventArgs.Data);
            var guildId = default(ulong);
            if (response is PlayerResponse playerResponse)
                guildId = playerResponse.GuildId;

            PlayerCache.TryGetValue(guildId, out var player);

            switch (response.EventType)
            {
                case EventType.Metrics:
                    break;

                case EventType.TrackException:
                    //var errored = response.Data.As<TrackErrorEventArgs>();
                    //OnTrackError?.Invoke(errored)
                    //.ConfigureAwait(false);
                    break;

                case EventType.TrackFinished:
                    //var endInfo = response.Data.As<TrackEndedEventArgs>();
                    //OnTrackEnded?.Invoke(endInfo)
                    //.ConfigureAwait(false);
                    break;

                case EventType.TrackUpdate:
                    //var update = response.Data.As<TrackUpdateEventArgs>();
                    //OnTrackUpdate?.Invoke(update)
                    //.ConfigureAwait(false);

                    player.LastUpdate = DateTimeOffset.Now;
                    break;
            }

            await Task.Delay(0)
                .ConfigureAwait(false);
        }
    }
}
