using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Victoria.Entities;
using Victoria.Entities.Enums;

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
        public bool IsConnected { get; }

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

        private readonly Sockeon _sockeon;
        private readonly HttpClient _httpClient;
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
            return true;
        }


        private Task OnSocketDisconnected(Exception arg)
        {
            throw new NotImplementedException();
        }

        private Task OnVoiceServerUpdated(SocketVoiceServer arg)
        {
            throw new NotImplementedException();
        }

        private Task OnVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            throw new NotImplementedException();
        }
    }
}