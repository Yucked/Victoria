using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Socks.EventArgs;
using Victoria.Common;
using Victoria.Lavalink.EventArgs;
using Victoria.Lavalink.Payloads;
using Victoria.Lavalink.Responses.Rest;
using Victoria.Lavalink.Responses.WebSocket;

namespace Victoria.Lavalink
{
    /// <summary>
    /// </summary>
    public class LavaNode : BaseClient<LavaPlayer, LavaTrack>
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

        private readonly LavaConfig _config;

        /// <inheritdoc />
        public LavaNode(DiscordSocketClient socketClient, LavaConfig config) : base(socketClient, config)
        {
            _config = config;
            socketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
            socketClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;
            Sock.OnReceive += OnReceiveAsync;
        }

        /// <inheritdoc />
        public LavaNode(DiscordShardedClient shardedClient, LavaConfig config) : base(shardedClient, config)
        {
            _config = config;
            shardedClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
            shardedClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;
            Sock.OnReceive += OnReceiveAsync;
        }

        /// <inheritdoc />
        public override async Task ConnectAsync()
        {
            var shards = SocketClient switch
            {
                DiscordSocketClient socketClient => await socketClient.GetRecommendedShardCountAsync()
                    .ConfigureAwait(false),
                DiscordShardedClient shardedClient => shardedClient.Shards.Count,
                _                                  => 1
            };

            Sock.AddHeader("User-Id", $"{SocketClient.CurrentUser.Id}");
            Sock.AddHeader("Num-Shards", $"{shards}");
            Sock.AddHeader("Authorization", _config.Authorization);

            if (_config.EnableResume)
                Sock.AddHeader("Resume-Key", _config.ResumeKey);

            await base.ConnectAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<LavaPlayer> JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = default)
        {
            Ensure.NotNull(voiceChannel);

            if (PlayerCache.TryGetValue(voiceChannel.GuildId, out var player))
                return player;

            await voiceChannel.ConnectAsync(_config.SelfDeaf, false, true)
                .ConfigureAwait(false);

            player = new LavaPlayer(Sock, voiceChannel, textChannel);
            PlayerCache.TryAdd(voiceChannel.GuildId, player);
            return player;
        }

        /// <inheritdoc />
        public override async Task MoveAsync(IVoiceChannel voiceChannel)
        {
            Ensure.NotNull(voiceChannel);

            if (!Volatile.Read(ref RefConnected))
                Throw.InvalidOperation("Can't execute this operation when client isn't connected.");

            if (!PlayerCache.TryGetValue(voiceChannel.GuildId, out var player))
                Throw.InvalidOperation($"No player was found for {voiceChannel.Guild.Name}.");

            if (player.VoiceChannel.Id == voiceChannel.Id)
                Throw.InvalidOperation("Connected and new voice channel ids are the same.");

            var payload = new DestroyPayload(voiceChannel.GuildId);
            await Sock.SendAsync(payload)
                .ConfigureAwait(false);

            await player.VoiceChannel.DisconnectAsync()
                .ConfigureAwait(false);

            await voiceChannel.ConnectAsync(_config.SelfDeaf, false, true)
                .ConfigureAwait(false);

            Log(LogSeverity.Info, nameof(Lavalink), $"Moved {voiceChannel.GuildId} player to {voiceChannel.Name}.");
        }

        /// <summary>
        ///     Searches YouTube for your query.
        /// </summary>
        /// <param name="query">Your search terms.</param>
        /// <returns>
        ///     <see cref="SearchResponse" />
        /// </returns>
        public Task<SearchResponse> SearchYouTubeAsync(string query)
        {
            Ensure.NotNull(query);
            return SearchAsync($"ytsearch:{query}");
        }

        /// <summary>
        ///     Searches SoundCloud for your query.
        /// </summary>
        /// <param name="query">Your search terms.</param>
        /// <returns>
        ///     <see cref="SearchResponse" />
        /// </returns>
        public Task<SearchResponse> SearchSoundCloudAsync(string query)
        {
            Ensure.NotNull(query);
            return SearchAsync($"scsearch:{query}");
        }

        /// <summary>
        ///     Performs search on all enabled sources in configuration.
        /// </summary>
        /// <param name="query">Your search terms.</param>
        /// <returns>
        ///     <see cref="SearchResponse" />
        /// </returns>
        public async Task<SearchResponse> SearchAsync(string query)
        {
            Ensure.NotNull(query);
            RestClient.WithHeader("Authorization", _config.Authorization);
            var request = await RestClient.RequestAsync(
                    $"http://{_config.Hostname}:{_config.Port}/loadtracks?identifier={WebUtility.UrlEncode(query)}")
                .ConfigureAwait(false);

            using var parse = JsonDocument.Parse(request);
            var root = parse.RootElement;

            var response = new SearchResponse(root);
            return response;
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

            var payload = new ServerUpdatePayload
            {
                SessionId = player.VoiceState.VoiceSessionId,
                VoiceServerPayload = new VoiceServerPayload
                {
                    Token = voiceServer.Token,
                    Endpoint = voiceServer.Endpoint,
                    GuildId = $"{voiceServer.Guild.Id}"
                }
            };

            await Sock.SendAsync(payload)
                .ConfigureAwait(false);
        }

        private async Task OnReceiveAsync(ReceivedEventArgs eventArgs)
        {
            if (eventArgs.DataSize == 0)
            {
                Log(LogSeverity.Warning, nameof(Lavalink), "Received empty payload.");
                return;
            }

            Log(LogSeverity.Debug, nameof(Lavalink), eventArgs.Raw);
            var baseWsResponse = JsonSerializer.Deserialize<BaseWsResponse>(eventArgs.Data.Span);

            switch (baseWsResponse.Op)
            {
                case "playerUpdate":
                    var playerUpdate = JsonSerializer.Deserialize<PlayerUpdateResponse>(eventArgs.Data.Span);
                    if (!PlayerCache.TryGetValue(playerUpdate.GuildId, out var player))
                        return;

                    OnPlayerUpdated?.Invoke(new PlayerUpdateEventArgs(player, playerUpdate))
                        .ConfigureAwait(false);
                    break;

                case "stats":
                    var statsResponse = JsonSerializer.Deserialize<StatsResponse>(eventArgs.Data.Span);
                    OnStatsReceived?.Invoke(new StatsEventArgs(statsResponse))
                        .ConfigureAwait(false);
                    break;

                case "event":
                    var baseEventResponse = JsonSerializer.Deserialize<BaseEventResponse>(eventArgs.Data.Span);
                    if (!PlayerCache.TryGetValue(baseEventResponse.GuildId, out player))
                        return;

                    switch (baseEventResponse.EventType)
                    {
                        case "TrackEndEvent":
                            var endEvent = JsonSerializer.Deserialize<TrackEndEvent>(eventArgs.Data.Span);
                            OnTrackEnded?.Invoke(new TrackEndedEventArgs(player, endEvent))
                                .ConfigureAwait(false);
                            break;

                        case "TrackExceptionEvent":
                            var exceptionEvent = JsonSerializer.Deserialize<TrackExceptionEvent>(eventArgs.Data.Span);
                            OnTrackException?.Invoke(new TrackExceptionEventArgs(player, exceptionEvent))
                                .ConfigureAwait(false);
                            break;

                        case "TrackStuckEvent":
                            var stuckEvent = JsonSerializer.Deserialize<TrackStuckEvent>(eventArgs.Data.Span);
                            OnTrackStuck?.Invoke(new TrackStuckEventArgs(player, stuckEvent))
                                .ConfigureAwait(false);
                            break;

                        case "WebSocketClosedEvent":
                            var closedEvent = JsonSerializer.Deserialize<WebSocketClosedEvent>(eventArgs.Data.Span);
                            OnWebSocketClosed?.Invoke(new WebSocketClosedEventArgs(closedEvent))
                                .ConfigureAwait(false);
                            break;

                        default:
                            Log(LogSeverity.Warning, nameof(Lavalink),
                                $"{baseEventResponse.EventType} event type is not handled.");
                            break;
                    }

                    break;

                default:
                    Log(LogSeverity.Warning, nameof(Lavalink), $"{baseWsResponse.Op} op code is not handled.");
                    break;
            }

            await Task.Delay(0)
                .ConfigureAwait(false);
        }
    }
}
