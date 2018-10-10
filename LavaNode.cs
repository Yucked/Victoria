using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Victoria.Objects;
using Victoria.Objects.Enums;
using Victoria.Objects.Stats;
using Victoria.Payloads;

namespace Victoria
{
    public sealed class LavaNode
    {
        internal LavaNode(BaseDiscordClient baseClient, LavaConfig config)
        {
            Config = config;
            BaseClient = baseClient;
            LavaSocket = new LavaSocket();
            LavaSocket.Log += InvokeLog;
            Rest = new HttpClient();
            Rest.DefaultRequestHeaders.Add("Authorization", Config.Authorization);
            Statistics = new LavaStats();
            Players = new ConcurrentDictionary<ulong, LavaPlayer>();

            switch (BaseClient)
            {
                case DiscordSocketClient socketClient:
                    socketClient.VoiceServerUpdated += OnVSU;
                    socketClient.UserVoiceStateUpdated += OnUVSU;
                    socketClient.Disconnected += OnSocketDisconnected;
                    break;
                case DiscordShardedClient shardClient:
                    shardClient.VoiceServerUpdated += OnVSU;
                    shardClient.UserVoiceStateUpdated += OnUVSU;
                    shardClient.ShardDisconnected += OnShardDisconnected;
                    break;
            }
        }

        private HttpClient Rest { get; }
        private LavaConfig Config { get; }
        internal LavaSocket LavaSocket { get; }
        private BaseDiscordClient BaseClient { get; }
        private SocketVoiceState VoiceState { get; set; }
        internal ConcurrentDictionary<ulong, LavaPlayer> Players { get; }

        /// <summary>
        ///     Lavalink Server Statistics.
        /// </summary>
        public LavaStats Statistics { get; }

        /// <summary>
        ///     Check if the node is connected to Lavalink.
        /// </summary>
        public bool IsConnected => LavaSocket.IsConnected;

        /// <summary>
        ///     Lavalink Log.
        /// </summary>
        public event Action<LavaLog> Log;

        /// <summary>
        ///     Fires when a track is stuck.
        /// </summary>
        public event Action<LavaPlayer, LavaTrack, long> Stuck;

        /// <summary>
        ///     Fires when Lavalink throws an exception.
        /// </summary>
        public event Action<LavaPlayer, LavaTrack, string> Exception;

        /// <summary>
        ///     Fires when LavaPlayer is updated.
        /// </summary>
        public event Action<LavaPlayer, LavaTrack, TimeSpan> Updated;

        /// <summary>
        ///     Fires when a track has finished playing.
        /// </summary>
        public event Action<LavaPlayer, LavaTrack, TrackReason> Finished;

        internal async Task StartAsync()
        {
            InvokeLog(LogSeverity.Verbose,
                $"Initializing node {Config.Socket.Host}:{Config.Socket.Port} connection...");
            var shards = await GetShardsAsync();
            LavaSocket.Connect(Config, BaseClient.CurrentUser.Id, shards);
            LavaSocket.PureSocket.OnMessage += OnMessage;
        }

        /// <summary>
        ///     Disconnects
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            foreach (var connection in Players)
                await connection.Value.DisconnectAsync();

            Players.Clear();
            LavaSocket.Disconnect();
            InvokeLog(LogSeverity.Info, $"Node {Config.Socket.Host}:{Config.Socket.Port} disconnected.");
        }

        /// <summary>
        ///     Join A Voice Channel.
        /// </summary>
        /// <param name="voiceChannel">VoiceChannel That Needs To Be Connected To.</param>
        /// <param name="textChannel">TextChannel Where Updates Will Be Sent.</param>
        public async Task<LavaPlayer> JoinAsync(IVoiceChannel voiceChannel, IMessageChannel textChannel)
        {
            if (Players.ContainsKey(voiceChannel.GuildId))
                return Players[voiceChannel.GuildId];
            var player = new LavaPlayer(this, voiceChannel, textChannel);
            await voiceChannel.ConnectAsync(false, false, true);
            Players.TryAdd(voiceChannel.Guild.Id, player);
            InvokeLog(LogSeverity.Verbose, $"Connected to {player.VoiceChannel.Name}.");
            return player;
        }

        /// <summary>
        ///     Leaves A Voice Channel. Must Be Connected To Previously.
        /// </summary>
        /// <param name="guildId">Guild Id</param>
        public async Task<bool> LeaveAsync(ulong guildId)
        {
            if (!Players.ContainsKey(guildId))
                return false;
            Players.TryGetValue(guildId, out var player);
            await player.DisconnectAsync();
            Players.TryRemove(guildId, out _);
            InvokeLog(LogSeverity.Verbose, $"Disconnected from {player.VoiceChannel.Name}.");
            return true;
        }

        /// <summary>
        ///     Returns LavaPlayer For Specific Guild (Otherwise Null).
        /// </summary>
        /// <param name="guildId">Guild Id</param>
        public LavaPlayer GetPlayer(ulong guildId)
        {
            return Players.ContainsKey(guildId) ? Players[guildId] : null;
        }

        /// <summary>
        ///     Searches Youtube.
        /// </summary>
        /// <param name="query">Your Search Terms.</param>
        public Task<LavaResult> SearchYouTubeAsync(string query)
        {
            var ytQuery = WebUtility.UrlEncode($"ytsearch:{query}");
            var url = new Uri($"http://{Config.Rest.Host}:{Config.Rest.Port}/loadtracks?identifier={ytQuery}");
            InvokeLog(LogSeverity.Verbose, $"GET {url}");
            return ResolveTracksAsync(url);
        }

        /// <summary>
        ///     Searches SoundCloud.
        /// </summary>
        /// <param name="query">Your Search Terms.</param>
        public Task<LavaResult> SearchSoundCloudAsync(string query)
        {
            var scQuery = WebUtility.UrlEncode($"scsearch:{query}");
            var url = new Uri($"http://{Config.Rest.Host}:{Config.Rest.Port}/loadtracks?identifier={scQuery}");
            InvokeLog(LogSeverity.Verbose, $"GET {url}");
            return ResolveTracksAsync(url);
        }

        /// <summary>
        ///     Performs A Broad Search.
        /// </summary>
        /// <param name="uri">URL</param>
        public Task<LavaResult> GetTracksAsync(Uri uri)
        {
            var url = new Uri(
                $"http://{Config.Rest.Host}:{Config.Rest.Port}/loadtracks?identifier={WebUtility.UrlEncode($"{uri}")}");
            InvokeLog(LogSeverity.Verbose, $"GET {url}");
            return ResolveTracksAsync(url);
        }

        private async Task OnUVSU(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (user.Id != BaseClient.CurrentUser.Id) return;

            switch (oldState)
            {
                case var state when state.VoiceChannel is null && !(newState.VoiceChannel is null):
                    VoiceState = newState;
                    InvokeLog(LogSeverity.Verbose, "Voice state updated.");
                    break;

                case var state when !(state.VoiceChannel is null) && newState.VoiceChannel is null:
                    if (!Players.TryGetValue(state.VoiceChannel.Guild.Id, out var oldPlayer))
                        return;
                    await oldPlayer.DisconnectAsync().ConfigureAwait(false);
                    Players.TryRemove(state.VoiceChannel.Guild.Id, out _);
                    LavaSocket.SendPayload(new DestroyPayload(state.VoiceChannel.Guild.Id));
                    break;

                case var state when state.VoiceChannel?.Id != newState.VoiceChannel?.Id:
                    if (!Players.TryGetValue(state.VoiceChannel.Guild.Id, out var updatePlayer))
                        return;
                    updatePlayer.VoiceChannel = newState.VoiceChannel;
                    Players.TryUpdate(state.VoiceChannel.Guild.Id, updatePlayer, updatePlayer);
                    InvokeLog(LogSeverity.Verbose, $"{updatePlayer.Guild.Id} voice channel updated.");
                    break;
            }
        }

        private Task OnVSU(SocketVoiceServer server)
        {
            if (!server.Guild.HasValue)
                return Task.CompletedTask;

            if (!Players.TryGetValue(server.Guild.Id, out _))
                return Task.CompletedTask;

            var voiceUpdate = new VoicePayload(server, VoiceState);
            LavaSocket.SendPayload(voiceUpdate);
            return Task.CompletedTask;
        }

        private async Task OnShardDisconnected(Exception exc, DiscordSocketClient client)
        {
            foreach (var guild in client.Guilds)
            {
                if (!Players.ContainsKey(guild.Id)) continue;
                await Players[guild.Id].DisconnectAsync();
                Players.TryRemove(guild.Id, out _);
            }

            InvokeLog(LogSeverity.Error, $"Disconnected from shard #{client.ShardId}.", exc);
        }

        private async Task OnSocketDisconnected(Exception exc)
        {
            foreach (var connection in Players)
                await connection.Value.DisconnectAsync();
            Players.Clear();
            InvokeLog(LogSeverity.Error, "Disconnected from Discord.", exc);
        }

        #region Private Methods

        private void OnMessage(string message)
        {
            ulong guildId = 0;
            var data = JObject.Parse(message);

            switch ($"{data["op"]}")
            {
                case "playerUpdate":
                    guildId = ulong.Parse($"{data["guildId"]}");
                    var state = data["state"].ToObject<LavaState>();
                    if (Players.TryGetValue(guildId, out var player))
                        UpdatePlayer(player, state);
                    InvokeLog(LogSeverity.Verbose, "Received player update.");
                    break;

                case "stats":
                    var stats = data.ToObject<Server>();
                    Statistics.Update(stats);
                    InvokeLog(LogSeverity.Verbose, "Received stats update.");
                    break;

                case "event":
                    var eventType = data["type"].ToObject<EventType>();
                    switch (eventType)
                    {
                        case EventType.TrackEndEvent:
                            guildId = ulong.Parse($"{data["guildId"]}");
                            var reason = default(TrackReason);
                            switch ($"{data["reason"]}")
                            {
                                case "FINISHED":
                                    reason = TrackReason.Finished;
                                    break;
                                case "LOAD_FAILED":
                                    reason = TrackReason.LoadFailed;
                                    break;
                                case "STOPPED":
                                    reason = TrackReason.Stopped;
                                    break;
                                case "REPLACED":
                                    reason = TrackReason.Replaced;
                                    break;
                                case "CLEANUP":
                                    reason = TrackReason.Cleanup;
                                    break;
                            }

                            if (Players.TryGetValue(guildId, out var eventPlayer))
                                TrackUpdate(new TrackFinishData
                                {
                                    Reason = reason,
                                    LavaPlayer = eventPlayer,
                                    Track = Util.DecodeTrack($"{data["track"]}")
                                });

                            break;

                        case EventType.TrackExceptionEvent:
                            if (Players.TryGetValue(guildId, out var stuck))
                                StuckUpdate(new TrackStuckData
                                {
                                    LavaPlayer = stuck,
                                    Track = Util.DecodeTrack($"{data["track"]}"),
                                    Threshold = long.Parse($"{data["thresholdMs"]}")
                                });
                            break;

                        case EventType.TrackStuckEvent:
                            if (Players.TryGetValue(guildId, out var exc))
                                ExceptionUpdate(new TrackExceptionData
                                {
                                    LavaPlayer = exc,
                                    Error = $"{data["error"]}",
                                    Track = Util.DecodeTrack($"{data["track"]}")
                                });
                            break;
                    }

                    break;

                default:
                    InvokeLog(LogSeverity.Info, "Unknown OP Code.");
                    break;
            }
        }

        private async Task<LavaResult> ResolveTracksAsync(Uri uri)
        {
            string json;
            using (var req = await Rest.GetAsync(uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Encoding.UTF8))
            {
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            var data = JToken.Parse(json);
            var tracks = new HashSet<LavaTrack>();
            switch (data)
            {
                case JArray jArray:
                    foreach (var array in jArray)
                    {
                        var aTrack = array["info"].ToObject<LavaTrack>();
                        aTrack.TrackString = $"{array["track"]}";
                        tracks.Add(aTrack);
                    }

                    return new LavaResult
                    {
                        Tracks = tracks,
                        PlaylistInfo = default,
                        LoadResultType = tracks.Count == 0 ? LoadResultType.LoadFailed : LoadResultType.TrackLoaded
                    };
                case JObject jObject:
                {
                    var jsonArray = jObject["tracks"] as JArray;
                    var info = jObject.ToObject<LavaResult>();
                    foreach (var item in jsonArray)
                    {
                        var track = item["info"].ToObject<LavaTrack>();
                        track.TrackString = $"{item["track"]}";
                        tracks.Add(track);
                    }

                    info.Tracks = tracks;
                    return info;
                }
                default: return null;
            }
        }

        private void InvokeLog(LogSeverity severity, string message = null, Exception exc = null)
        {
            if (severity >= Config.Severity) return;
            var log = new LavaLog(severity, message, exc);
            Log?.Invoke(log);
        }

        private async Task<int> GetShardsAsync()
        {
            switch (BaseClient)
            {
                case DiscordSocketClient client:
                    return await client.GetRecommendedShardCountAsync();
                case DiscordShardedClient shardedClient:
                    return shardedClient.Shards.Count;
                default: return 1;
            }
        }

        internal void UpdatePlayer(LavaPlayer player, LavaState state)
        {
            player.LastUpdate = state.Time;
            player.Position = state.Position;
            Updated?.Invoke(player, player.CurrentTrack, player.Position);
        }

        internal void TrackUpdate(TrackFinishData data)
        {
            data.LavaPlayer.CurrentTrack = null;
            Finished?.Invoke(data.LavaPlayer, data.Track, data.Reason);
        }

        internal void StuckUpdate(TrackStuckData data)
        {
            Stuck?.Invoke(data.LavaPlayer, data.Track, data.Threshold);
        }

        internal void ExceptionUpdate(TrackExceptionData data)
        {
            Exception?.Invoke(data.LavaPlayer, data.Track, data.Error);
        }

        #endregion
    }
}