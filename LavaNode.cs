using Discord;
using Discord.Rest;
using Discord.WebSocket;
using HyperEx;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Victoria.Misc;
using Victoria.Objects;
using Victoria.Objects.Enums;
using Victoria.Objects.Stats;
using Victoria.Payloads;

namespace Victoria
{
    public sealed class LavaNode : IDisposable
    {
        private int _retryInterval = 3000;
        private readonly HttpClient _rest;
        private readonly Lavalink _lavalink;
        private readonly LavaConfig _config;
        private SocketVoiceState _voiceState;        
        private readonly BaseDiscordClient _baseClient;

        internal readonly LavaSocket _lavaSocket;
        internal ConcurrentDictionary<ulong, LavaPlayer> _players;

        /// <summary>
        ///     _lavalink Server Statistics.
        /// </summary>
        public LavaStats Statistics { get; }

        /// <summary>
        ///     Check if the node is connected to _lavalink.
        /// </summary>
        public bool IsConnected => _lavaSocket.IsConnected;

        /// <summary>
        ///     Fires when a track is stuck.
        /// </summary>
        public event AsyncEvent<LavaPlayer, LavaTrack, long> Stuck;

        /// <summary>
        ///     Fires when _lavalink throws an exception.
        /// </summary>
        public event AsyncEvent<LavaPlayer, LavaTrack, string> Exception;

        /// <summary>
        ///     Fires when LavaPlayer is updated.
        /// </summary>
        public event AsyncEvent<LavaPlayer, LavaTrack, TimeSpan> Updated;

        /// <summary>
        ///     Fires when a track has finished playing.
        /// </summary>
        public event AsyncEvent<LavaPlayer, LavaTrack, TrackReason> Finished;


        internal LavaNode(BaseDiscordClient baseClient, LavaSocket socket, LavaConfig config)
        {
            _config = config;
            _baseClient = baseClient;
            _lavaSocket = socket;
            _lavalink = _lavaSocket._lavalink;            
            _rest = new HttpClient();
            _rest.DefaultRequestHeaders.Add("Authorization", _config.Authorization);
            Statistics = new LavaStats();
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();

            switch (_baseClient)
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

        internal async Task StartAsync()
        {
            _lavaSocket.OnReceive += OnMessage;
            _lavaSocket.OnClose += OnClose;
            await _lavaSocket.ConnectAsync();
        }

        /// <summary>
        ///     Disconnects
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            foreach (var connection in _players)
                await connection.Value.DisconnectAsync();

            _players.Clear();
            Asyncs.RunSync(_lavaSocket.DisconnectAsync);
        }

        /// <summary>
        ///     Join A Voice Channel.
        /// </summary>
        /// <param name="voiceChannel">VoiceChannel That Needs To Be Connected To.</param>
        /// <param name="textChannel">TextChannel Where Updates Will Be Sent.</param>
        public async Task<LavaPlayer> JoinAsync(IVoiceChannel voiceChannel, IMessageChannel textChannel = null)
        {
            if (_players.ContainsKey(voiceChannel.GuildId))
                return _players[voiceChannel.GuildId];
            var player = new LavaPlayer(this, voiceChannel, textChannel);
            await voiceChannel.ConnectAsync(false, false, true);
            _players.TryAdd(voiceChannel.Guild.Id, player);
            return player;
        }

        /// <summary>
        ///     Leaves A Voice Channel. Must Be Connected To Previously.
        /// </summary>
        /// <param name="guildId">Guild Id</param>
        public async Task<bool> LeaveAsync(ulong guildId)
        {
            if (!_players.ContainsKey(guildId))
                return false;
            _players.TryGetValue(guildId, out var player);
            await player.DisconnectAsync();
            return _players.TryRemove(guildId, out _);
        }

        /// <summary>
        ///     Returns LavaPlayer For Specific Guild (Otherwise Null).
        /// </summary>
        /// <param name="guildId">Guild Id</param>
        public LavaPlayer GetPlayer(ulong guildId)
        {
            return _players.ContainsKey(guildId) ? _players[guildId] : null;
        }

        /// <summary>
        ///     Searches Youtube.
        /// </summary>
        /// <param name="query">Your Search Terms.</param>
        public Task<LavaResult> SearchYouTubeAsync(string query)
        {
            var ytQuery = WebUtility.UrlEncode($"ytsearch:{query}");
            var url = new Uri($"http://{_config.Endpoint.Host}:{_config.Endpoint.Port}/loadtracks?identifier={ytQuery}");
            return ResolveTracksAsync(url);
        }

        /// <summary>
        ///     Searches SoundCloud.
        /// </summary>
        /// <param name="query">Your Search Terms.</param>
        public Task<LavaResult> SearchSoundCloudAsync(string query)
        {
            var scQuery = WebUtility.UrlEncode($"scsearch:{query}");
            var url = new Uri($"http://{_config.Endpoint.Host}:{_config.Endpoint.Port}/loadtracks?identifier={scQuery}");
            return ResolveTracksAsync(url);
        }

        /// <summary>
        ///     Performs A Broad Search.
        /// </summary>
        /// <param name="uri">URL</param>
        public Task<LavaResult> GetTracksAsync(Uri uri)
        {
            var url = new Uri(
                $"http://{_config.Endpoint.Host}:{_config.Endpoint.Port}/loadtracks?identifier={WebUtility.UrlEncode($"{uri}")}");
            return ResolveTracksAsync(url);
        }

        private async Task OnUVSU(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (user.Id != _baseClient.CurrentUser.Id) return;

            switch (oldState)
            {
                case var state when state.VoiceChannel is null && !(newState.VoiceChannel is null):
                    _voiceState = newState;
                    _lavalink.LogDebug($"Voice state updated. Voice session Id: {_voiceState.VoiceSessionId}");
                    break;

                case var state when !(state.VoiceChannel is null) && newState.VoiceChannel is null:
                    if (!_players.TryGetValue(state.VoiceChannel.Guild.Id, out var oldPlayer))
                        return;
                    await oldPlayer.DisconnectAsync().ConfigureAwait(false);
                    _players.TryRemove(state.VoiceChannel.Guild.Id, out _);
                    _lavaSocket.SendPayload(new DestroyPayload(state.VoiceChannel.Guild.Id));
                    break;

                case var state when state.VoiceChannel?.Id != newState.VoiceChannel?.Id:
                    if (!_players.TryGetValue(state.VoiceChannel.Guild.Id, out var updatePlayer))
                        return;
                    updatePlayer.VoiceChannel = newState.VoiceChannel;
                    _players.TryUpdate(state.VoiceChannel.Guild.Id, updatePlayer, updatePlayer);
                    _lavalink.LogInfo(
                        $"Moved from {state.VoiceChannel.Name} to {newState.VoiceChannel.Name}.");
                    break;
            }
        }

        private Task OnVSU(SocketVoiceServer server)
        {
            if (!server.Guild.HasValue)
                return Task.CompletedTask;

            if (!_players.TryGetValue(server.Guild.Id, out _))
                return Task.CompletedTask;

            var voiceUpdate = new VoicePayload(server, _voiceState);
            _lavaSocket.SendPayload(voiceUpdate);
            return Task.CompletedTask;
        }

        private async Task OnShardDisconnected(Exception exc, DiscordSocketClient client)
        {
            foreach (var guild in client.Guilds)
            {
                if (!_players.ContainsKey(guild.Id)) continue;
                await _players[guild.Id].DisconnectAsync();
                _players.TryRemove(guild.Id, out _);
            }

            _lavalink.LogError(null, exc);
        }

        private async Task OnSocketDisconnected(Exception exc)
        {
            foreach (var connection in _players)
                await connection.Value.DisconnectAsync();
            _players.Clear();
            _lavalink.LogError(null, exc);
        }

        private void OnMessage(string message)
        {
            ulong guildId = 0;
            var data = JObject.Parse(message);

            switch ($"{data["op"]}")
            {
                case "playerUpdate":
                    guildId = ulong.Parse($"{data["guildId"]}");
                    var state = data["state"].ToObject<LavaState>();
                    if (_players.TryGetValue(guildId, out var player))
                        UpdatePlayer(player, state);
                    _lavalink.LogDebug("Received Player Update.");
                    break;

                case "stats":
                    var stats = data.ToObject<Server>();
                    Statistics.Update(stats);
                    _lavalink.LogDebug("Received Stats Update.");
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

                            if (_players.TryGetValue(guildId, out var eventPlayer))
                                TrackUpdate(new TrackFinishData
                                {
                                    Reason = reason,
                                    LavaPlayer = eventPlayer,
                                    Track = TrackHelper.DecodeTrack($"{data["track"]}")
                                });

                            break;

                        case EventType.TrackExceptionEvent:
                            if (_players.TryGetValue(guildId, out var stuck))
                                StuckUpdate(new TrackStuckData
                                {
                                    LavaPlayer = stuck,
                                    Track = TrackHelper.DecodeTrack($"{data["track"]}"),
                                    Threshold = long.Parse($"{data["thresholdMs"]}")
                                });
                            break;

                        case EventType.TrackStuckEvent:
                            if (_players.TryGetValue(guildId, out var exc))
                                ExceptionUpdate(new TrackExceptionData
                                {
                                    LavaPlayer = exc,
                                    Error = $"{data["error"]}",
                                    Track = TrackHelper.DecodeTrack($"{data["track"]}")
                                });
                            break;
                    }

                    break;

                default:
                    _lavalink.LogDebug("Unknown OP Code.");
                    break;
            }
        }

        private async Task OnClose()
        {
            if (_lavaSocket._tries >= _config.MaxTries && _config.MaxTries != 0)
            {
                _lavalink.LogInfo("Max numbers of tries reached.");
                return;
            }
            else
            {
                if (_lavaSocket.IsConnected) return;
                _lavaSocket._tries++;
                _retryInterval += 1500;
                _lavalink.LogInfo($"Reconnect attempt #{_lavaSocket._tries}. Waiting {_retryInterval}ms before reconnecting.");
                await Task.Delay(_retryInterval).ContinueWith(_ => _lavaSocket.ConnectAsync());
            }
        }

        private async Task<LavaResult> ResolveTracksAsync(Uri uri)
        {
            string json;
            using (var req = await _rest.GetAsync(uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Encoding.UTF8))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

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

        public void Dispose()
        {
            _rest.Dispose();
            _players.Clear();
            _players = null;
        }
    }
}