using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Victoria.Entities;
using Victoria.Entities.Enums;
using Victoria.Entities.Payloads;

namespace Victoria
{
    public sealed class LavaNode
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Func<object, Task> StatsUpdated;

        /// <summary>
        /// 
        /// </summary>
        public Func<LavaPlayer, LavaTrack, long, Task> TrackStuck;

        /// <summary>
        /// 
        /// </summary>
        public Func<LavaPlayer, LavaTrack, string, Task> TrackException;

        /// <summary>
        /// 
        /// </summary>
        public Func<LavaPlayer, LavaTrack, TimeSpan, Task> PlayerUpdated;

        /// <summary>
        /// 
        /// </summary>
        public Func<LavaPlayer, LavaTrack, TrackReason, Task> TrackFinished;

        internal readonly Sockeon _sockeon;
        private readonly HttpClient _httpClient;
        private SocketVoiceState _socketVoiceState;
        private readonly Configuration _configuration;
        private readonly BaseDiscordClient _baseDiscordClient;
        private readonly ConcurrentDictionary<ulong, LavaPlayer> _players;

        internal LavaNode(string name, BaseDiscordClient baseDiscordClient, Configuration configuration)
        {
            Name = name;
            _sockeon = new Sockeon(configuration);
            _baseDiscordClient = baseDiscordClient;
            _configuration = configuration;
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();

            _httpClient = new HttpClient(new HttpClientHandler
            {
                UseCookies = false,
                Proxy = configuration.Proxy,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            {
                BaseAddress = new Uri($"http://{configuration.Host}:{configuration.Port}/loadtracks")
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Victoria");
            _httpClient.DefaultRequestHeaders.Add("Authorization", configuration.Authorization);
            _sockeon.OnMessage += OnMessage;

            switch (baseDiscordClient)
            {
                case DiscordSocketClient socketClient:
                    socketClient.Disconnected += OnSocketDisconnected;
                    socketClient.VoiceServerUpdated += OnVoiceServerUpdated;
                    socketClient.UserVoiceStateUpdated += OnVoiceStateUpdated;
                    break;

                case DiscordShardedClient shardedClient:
                    shardedClient.VoiceServerUpdated += OnVoiceServerUpdated;
                    shardedClient.UserVoiceStateUpdated += OnVoiceStateUpdated;
                    break;
            }

            IsConnected = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal Task StartAsync()
            => _sockeon.ConnectAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal Task StopAsync()
        {
            foreach (var player in _players.Values)
            {
            }

            IsConnected = false;
            return Task.CompletedTask;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="voiceChannel"></param>
        /// <param name="messageChannel"></param>
        /// <returns></returns>
        public async Task<LavaPlayer> ConnectAsync(IVoiceChannel voiceChannel, IMessageChannel messageChannel)
        {
            if (_players.TryGetValue(voiceChannel.GuildId, out var player))
                return player;

            return player;
        }

        
        
        private bool OnMessage(string data)
        {
            var parsed = JObject.Parse(data);
            var guildId = ulong.Parse($"{parsed.GetValue("guildId")}");

            switch ($"{parsed.GetValue("op")}")
            {
                case "playerUpdate":
                    var state = parsed.GetValue("state").ToObject<LavaState>();
                    UpdatePlayerInformation(guildId, state);
                    break;

                case "stats":

                    break;

                case "event":

                    break;

                default:
                    //TODO: Log uknown op code.
                    break;
            }

            return true;
        }

        private async Task<string> ResolveRequestAsync(string query)
        {
            
        }

        // Event stuff

        private void UpdatePlayerInformation(ulong guildId, LavaState state)
        {
            if (!_players.TryGetValue(guildId, out var old)) return;
            old.CurrentTrack.Position = state.Position;
            old.LastUpdate = state.Time;
            _players.TryUpdate(guildId, old, old);
            PlayerUpdated(old, old.CurrentTrack, state.Position);
        }

        private Task OnSocketDisconnected(Exception exception)
        {
            throw new NotImplementedException();
        }

        private Task OnVoiceServerUpdated(SocketVoiceServer socketVoiceServer)
        {
            throw new NotImplementedException();
        }

        private async Task OnVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (user.Id != _baseDiscordClient.CurrentUser.Id) return;

            switch (oldState)
            {
                case var state when state.VoiceChannel is null && !(newState.VoiceChannel is null):
                    _socketVoiceState = newState;
                    break;

                case var state when !(state.VoiceChannel is null) && newState.VoiceChannel is null:
                    if (!_players.TryGetValue(state.VoiceChannel.Guild.Id, out var oldPlayer))
                        return;
                    await oldPlayer.TryDisconnectAsync().ConfigureAwait(false);
                    _players.TryRemove(state.VoiceChannel.Guild.Id, out _);
                    await _sockeon.SendPayloadAsync(new DestroyPayload(state.VoiceChannel.Guild.Id))
                        .ConfigureAwait(false);
                    break;

                case var state when state.VoiceChannel?.Id != newState.VoiceChannel?.Id:
                    if (!_players.TryGetValue(state.VoiceChannel.Guild.Id, out var updatePlayer))
                        return;
                    updatePlayer.VoiceChannel = newState.VoiceChannel;
                    _players.TryUpdate(state.VoiceChannel.Guild.Id, updatePlayer, updatePlayer);
                    break;
            }
        }
    }
}