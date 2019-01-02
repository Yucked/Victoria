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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Victoria.Entities;
using Victoria.Entities.Enums;
using Victoria.Entities.Payloads;
using Victoria.Entities.Statistics;
using Victoria.Utilities;

namespace Victoria
{
    /// <summary>
    /// Represents a <see cref="BaseDiscordClient"/> connection.
    /// </summary>
    public sealed class LavaNode
    {
        /// <summary>
        /// Name of current <see cref="LavaNode"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether this node is connected or not.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Keeps up to date with <see cref="StatsUpdated"/>.
        /// </summary>
        public Stats Stats { get; private set; }

        /// <summary>
        /// Fires when stats are sent from Lavalink server.
        /// </summary>
        public Func<Stats, Task> StatsUpdated;

        /// <summary>
        /// Fires when websocket is closed.
        /// </summary>
        public Func<int, string, bool, Task> SocketClosed;

        /// <summary>
        /// Fires when a track has timed out.
        /// </summary>
        public Func<LavaPlayer, LavaTrack, long, Task> TrackStuck;

        /// <summary>
        /// Fires when a track throws an exception.
        /// </summary>
        public Func<LavaPlayer, LavaTrack, string, Task> TrackException;

        /// <summary>
        /// Fires when player update is sent from lavalink server.
        /// </summary>
        public Func<LavaPlayer, LavaTrack, TimeSpan, Task> PlayerUpdated;

        /// <summary>
        /// Fires when any of the <see cref="TrackReason"/> 's are met.
        /// </summary>
        public Func<LavaPlayer, LavaTrack, TrackReason, Task> TrackFinished;


        private HttpClient _httpClient;
        internal SocketResolver Socket;
        private SocketVoiceState _socketVoiceState;
        private readonly Configuration _configuration;
        private readonly Func<LogMessage, Task> _log;
        private readonly BaseDiscordClient _baseDiscordClient;
        private readonly ConcurrentDictionary<ulong, LavaPlayer> _players;

        internal LavaNode(string name, BaseDiscordClient baseDiscordClient, Configuration configuration,
            Func<LogMessage, Task> log)
        {
            Name = name;
            _log = log;
            _configuration = configuration;
            _baseDiscordClient = baseDiscordClient;
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();
            Initialize(configuration);
            switch (baseDiscordClient)
            {
                case DiscordSocketClient socketClient:
                    socketClient.Disconnected += OnSocketDisconnected;
                    socketClient.VoiceServerUpdated += OnVoiceServerUpdated;
                    socketClient.UserVoiceStateUpdated += OnVoiceStateUpdated;
                    break;

                case DiscordShardedClient shardedClient:
                    shardedClient.ShardDisconnected += OnShardDisconnected;
                    shardedClient.VoiceServerUpdated += OnVoiceServerUpdated;
                    shardedClient.UserVoiceStateUpdated += OnVoiceStateUpdated;
                    break;
            }

            IsConnected = true;
        }

        internal void Initialize(Configuration configuration)
        {
            Socket = new SocketResolver(Name, configuration, _log);
            _httpClient = new HttpClient(new HttpClientHandler
            {
                UseCookies = false,
                Proxy = configuration.Proxy,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            {
                BaseAddress = new Uri($"http://{configuration.Host}:{configuration.Port}")
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Victoria");
            _httpClient.DefaultRequestHeaders.Add("Authorization", configuration.Authorization);
        }

        internal async Task StartAsync()
        {
            Socket.OnMessage += OnMessage;
            await Socket.ConnectAsync().ConfigureAwait(false);
        }

        internal async Task StopAsync()
        {
            foreach (var player in _players.Values)
            {
                await player.VoiceChannel.DisconnectAsync().ConfigureAwait(false);
                player.Dispose();
            }

            await Socket.DisconnectAsync().ConfigureAwait(false);
            Socket.Dispose();
            _httpClient.Dispose();
            IsConnected = false;
        }

        /// <summary>
        /// Connects to a voice channel and bounds given text channel for any updates.
        /// </summary>
        /// <param name="voiceChannel">Voice channel to connect to.</param>
        /// <param name="messageChannel">Bounded text channel.</param>
        /// <returns><see cref="LavaPlayer"/></returns>
        public async Task<LavaPlayer> ConnectAsync(IVoiceChannel voiceChannel, IMessageChannel messageChannel = null)
        {
            if (_players.TryGetValue(voiceChannel.GuildId, out var player))
                return player;

            var newPlayer = new LavaPlayer(this, voiceChannel, messageChannel);
            await voiceChannel.ConnectAsync(_configuration.SelfDeaf, false, true).ConfigureAwait(false);
            _players.TryAdd(voiceChannel.GuildId, newPlayer);
            return newPlayer;
        }

        /// <summary>
        /// Disconnects the player from voice channel and lavalink then removes it.
        /// </summary>
        /// <param name="guildId"></param>
        public async Task<bool> DisconnectAsync(ulong guildId)
        {
            if (!_players.TryGetValue(guildId, out var player))
                return false;
            player.VoiceChannel?.DisconnectAsync().ConfigureAwait(false);
            await Socket.SendPayloadAsync(new DestroyPayload(guildId)).ConfigureAwait(false);
            return _players.TryRemove(guildId, out _);
        }

        /// <summary>
        /// Moves a voice channel.
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="voiceChannel"></param>
        public async Task<LavaPlayer> MoveAsync(ulong guildId, IVoiceChannel voiceChannel)
        {
            if (!_players.TryGetValue(guildId, out var old))
                return null;
            await old.VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            await voiceChannel.ConnectAsync(_configuration.SelfDeaf, false, true).ConfigureAwait(false);
            old.VoiceChannel = voiceChannel;
            _players.TryUpdate(guildId, old, old);
            return old;
        }

        /// <summary>
        /// Fetches <see cref="LavaPlayer"/> for given guild otherwise null.
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns><see cref="LavaPlayer"/></returns>
        public LavaPlayer GetPlayer(ulong guildId)
            => _players.TryGetValue(guildId, out var player) ? player : null;

        /// <summary>
        /// Searches all of the sources specified in application.yml. Also accepts file path pointing to a playable file.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <returns><see cref="LavaResult"/></returns>
        public Task<LavaResult> GetTracksAsync(string query)
            => ResolveRequestAsync(WebUtility.UrlEncode(query));

        /// <summary>
        /// Performs a Youtube search for your query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <returns><see cref="LavaResult"/></returns>
        public Task<LavaResult> SearchYouTubeAsync(string query)
            => ResolveRequestAsync(WebUtility.UrlEncode($"ytsearch:{query}"));

        /// <summary>
        /// Performs a Soundcloud search for your query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <returns><see cref="LavaResult"/></returns>
        public Task<LavaResult> SearchSoundcloudAsync(string query)
            => ResolveRequestAsync(WebUtility.UrlEncode($"scsearch:{query}"));

        private bool OnMessage(string data)
        {
            var parsed = JObject.Parse(data);
            _log?.Invoke(LogResolver.Debug(Name, data)).ConfigureAwait(false);
            ulong guildId = 0;
            if (parsed.TryGetValue("guildId", out var value))
                guildId = ulong.Parse($"{value}");

            var opCode = $"{parsed.GetValue("op")}";
            switch (opCode)
            {
                case "playerUpdate":
                    var state = parsed.GetValue("state").ToObject<LavaState>();
                    UpdatePlayerInfo(guildId, state);
                    break;

                case "stats":
                    var stats = parsed.ToObject<Stats>();
                    StatsUpdated?.Invoke(stats);
                    Stats = stats;
                    break;

                case "event":
                    var evt = parsed.GetValue("type").ToObject<EventType>();
                    switch (evt)
                    {
                        case EventType.TrackEndEvent:
                            var trackReason = parsed.GetValue("reason").ToObject<TrackReason>();
                            TrackFinishedInfo(guildId, TrackResolver.DecodeTrack($"{parsed.GetValue("track")}"),
                                trackReason);
                            break;

                        case EventType.TrackExceptionEvent:
                            var error = $"{parsed.GetValue("error")}";
                            TrackExceptionInfo(guildId, TrackResolver.DecodeTrack($"{parsed.GetValue("track")}"),
                                error);
                            break;

                        case EventType.TrackStuckEvent:
                            var threshold = long.Parse($"{parsed.GetValue("thresholdMs")}");
                            TrackStuckInfo(guildId, TrackResolver.DecodeTrack($"{parsed.GetValue("track")}"),
                                threshold);
                            break;

                        case EventType.WebSocketClosedEvent:
                            var reason = $"{parsed.GetValue("reason")}";
                            var code = int.Parse($"{parsed.GetValue("code")}");
                            var byRemote = bool.Parse($"{parsed.GetValue("byRemote")}");
                            SocketClosed?.Invoke(code, reason, byRemote);
                            break;

                        default:
                            _log?.Invoke(LogResolver.Info(Name, $"Unhandled event type: {evt}."));
                            break;
                    }

                    break;

                default:
                    _log?.Invoke(LogResolver.Info(Name, $"Unhandled OP code: {opCode}."));
                    break;
            }

            return true;
        }

        private async Task<LavaResult> ResolveRequestAsync(string query)
        {
            string json;
            using (var req = await _httpClient.GetAsync($"/loadtracks?identifier={query}").ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Encoding.UTF8))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            var data = JToken.Parse(json);
            _log?.Invoke(LogResolver.Debug(Name, json));
            var tracks = new HashSet<LavaTrack>();
            switch (data)
            {
                case JArray jArray:
                    foreach (var arr in jArray)
                    {
                        var arrTrack = arr["info"].ToObject<LavaTrack>();
                        arrTrack.TrackString = $"{arr["track"]}";
                        tracks.Add(arrTrack);
                    }

                    return new LavaResult
                    {
                        Tracks = tracks,
                        PlaylistInfo = default,
                        LoadResultType = tracks.Count == 0 ? LoadResultType.LoadFailed : LoadResultType.TrackLoaded
                    };

                case JObject jObject:
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

                default:
                    return null;
            }
        }

        // Event stuff

        private void UpdatePlayerInfo(ulong guildId, LavaState state)
        {
            _log?.Invoke(LogResolver.Debug(Name, "Received player update."));
            if (!_players.TryGetValue(guildId, out var old)) return;
            old.CurrentTrack.Position = state.Position;
            old.LastUpdate = state.Time;
            _players.TryUpdate(guildId, old, old);
            PlayerUpdated?.Invoke(old, old.CurrentTrack, state.Position);
        }

        private void TrackFinishedInfo(ulong guildId, LavaTrack track, TrackReason reason)
        {
            _log?.Invoke(LogResolver.Debug(Name, "Track update received."));
            if (!_players.TryGetValue(guildId, out var old)) return;
            if (reason != TrackReason.Replaced)
                old.CurrentTrack = default;
            _players.TryUpdate(guildId, old, old);
            TrackFinished?.Invoke(old, track, reason);
        }

        private void TrackStuckInfo(ulong guildId, LavaTrack track, long threshold)
        {
            _log?.Invoke(LogResolver.Debug(Name, "Track stuck update received."));
            if (!_players.TryGetValue(guildId, out var old)) return;
            old.CurrentTrack = track;
            _players.TryUpdate(guildId, old, old);
            TrackStuck?.Invoke(old, track, threshold);
        }

        private void TrackExceptionInfo(ulong guildId, LavaTrack track, string reason)
        {
            _log?.Invoke(LogResolver.Debug(Name, "Received track exception update."));
            if (!_players.TryGetValue(guildId, out var old)) return;
            old.CurrentTrack = track;
            _players.TryUpdate(guildId, old, old);
            TrackException?.Invoke(old, track, reason);
        }

        private async Task OnSocketDisconnected(Exception exception)
        {
            foreach (var player in _players.Values)
            {
                await player.VoiceChannel.DisconnectAsync().ConfigureAwait(false);
                player.Dispose();
            }

            _players.Clear();
            _log?.Invoke(LogResolver.Error(Name, "Socket disconnected.", exception)).ConfigureAwait(false);
        }

        private async Task OnShardDisconnected(Exception exception, DiscordSocketClient socketClient)
        {
            foreach (var guild in socketClient.Guilds)
            {
                if (!_players.TryGetValue(guild.Id, out var player))
                    continue;
                await player.VoiceChannel.DisconnectAsync().ConfigureAwait(false);
                player.Dispose();
                _players.TryRemove(guild.Id, out _);
            }

            _log?.Invoke(LogResolver.Error(Name, "Discord shard lost connection.", exception)).ConfigureAwait(false);
        }

        private async Task OnVoiceServerUpdated(SocketVoiceServer socketVoiceServer)
        {
            if (!socketVoiceServer.Guild.HasValue)
                return;
            if (!_players.TryGetValue(socketVoiceServer.Guild.Id, out _))
                return;
            var voiceUpdate = new VoicePayload(socketVoiceServer, _socketVoiceState);
            await Socket.SendPayloadAsync(voiceUpdate).ConfigureAwait(false);
            _log?.Invoke(LogResolver.Debug(Name,
                $"Sent voice server payload. {JsonConvert.SerializeObject(voiceUpdate)}")).ConfigureAwait(false);
        }

        private async Task OnVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (user.Id != _baseDiscordClient.CurrentUser.Id) return;

            switch (oldState)
            {
                case var state when state.VoiceChannel is null && !(newState.VoiceChannel is null):
                    _socketVoiceState = newState;
                    _log?.Invoke(LogResolver.Debug(Name, $"Socket voice state updated {newState}"))
                        .ConfigureAwait(false);
                    break;

                case var state when !(state.VoiceChannel is null) && newState.VoiceChannel is null:
                    if (!_players.TryGetValue(state.VoiceChannel.Guild.Id, out var oldPlayer))
                        return;
                    oldPlayer?.VoiceChannel.DisconnectAsync().ConfigureAwait(false);
                    oldPlayer?.Dispose();
                    _players.TryRemove(state.VoiceChannel.Guild.Id, out _);
                    var payload = new DestroyPayload(state.VoiceChannel.Guild.Id);
                    await Socket.SendPayloadAsync(payload).ConfigureAwait(false);
                    _log?.Invoke(
                            LogResolver.Debug(Name, $"Sent destroy payload. {JsonConvert.SerializeObject(payload)}"))
                        .ConfigureAwait(false);
                    break;
            }
        }

        internal void Dispose()
        {
            IsConnected = false;
            _players.Clear();
            Socket.Dispose();
            _httpClient.Dispose();
        }
    }
}