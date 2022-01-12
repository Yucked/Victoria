using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;
using Victoria.Payloads;
using Victoria.Responses.Search;
using Victoria.WebSocket;
using Victoria.WebSocket.EventArgs;
using Victoria.Wrappers;

namespace Victoria {
    /// <summary>
    /// 
    /// </summary>
    public class AbstractLavaNode : AbstractLavaNode<ILavaPlayer>, ILavaNode {
        /// <inheritdoc />
        public AbstractLavaNode(NodeConfiguration nodeConfiguration, ILogger<ILavaNode> logger)
            : base(nodeConfiguration, logger) { }
    }

    /// <inheritdoc />
    public class AbstractLavaNode<TLavaPlayer> : AbstractLavaNode<TLavaPlayer, ILavaTrack>
        where TLavaPlayer : ILavaPlayer<ILavaTrack> {
        /// <inheritdoc />
        public AbstractLavaNode(NodeConfiguration nodeConfiguration, ILogger<ILavaNode> logger)
            : base(nodeConfiguration, logger) { }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public class AbstractLavaNode<TLavaPlayer, TLavaTrack> : ILavaNode<TLavaPlayer, TLavaTrack>
        where TLavaPlayer : ILavaPlayer<TLavaTrack>
        where TLavaTrack : ILavaTrack {
        /// <inheritdoc />
        public bool IsConnected
            => Volatile.Read(ref IsConnectedRef);

        /// <inheritdoc />
        public IReadOnlyCollection<TLavaPlayer> Players
            => PlayersCache.Values as IReadOnlyCollection<TLavaPlayer>;

        /// <summary>
        /// 
        /// </summary>
        protected DiscordClient DiscordClient { get; init; }

        /// <summary>
        /// 
        /// </summary>
        protected readonly ConcurrentDictionary<ulong, TLavaPlayer> PlayersCache;

        /// <summary>
        /// 
        /// </summary>
        protected bool IsConnectedRef;

        /// <summary>
        /// 
        /// </summary>
        protected readonly WebSocketClient WebSocketClient;

        private readonly ILogger _logger;
        private readonly NodeConfiguration _nodeConfiguration;
        private readonly ConcurrentDictionary<ulong, VoiceState> _voiceStates;

        /// <summary>
        /// 
        /// </summary>
        public AbstractLavaNode(NodeConfiguration nodeConfiguration, ILogger<ILavaNode> logger) {
            _nodeConfiguration = nodeConfiguration;
            _logger = logger;
            WebSocketClient = new WebSocketClient(nodeConfiguration.Hostname, nodeConfiguration.Port, "ws");
            PlayersCache = new ConcurrentDictionary<ulong, TLavaPlayer>();
            _voiceStates = new ConcurrentDictionary<ulong, VoiceState>();

            WebSocketClient.OnOpenAsync += OnOpenAsync;
            WebSocketClient.OnCloseAsync += OnCloseAsync;
            WebSocketClient.OnErrorAsync += OnErrorAsync;
            WebSocketClient.OnMessageAsync += OnMessageAsync;

            DiscordClient.OnVoiceServerUpdated = OnVoiceServerUpdated;
            DiscordClient.OnUserVoiceStateUpdated = OnUserVoiceStateUpdated;
        }

        /// <inheritdoc />
        public async ValueTask ConnectAsync() {
            if (Volatile.Read(ref IsConnectedRef)) {
                throw new InvalidOperationException(
                    $"A connection is already established. Please call {nameof(DisconnectAsync)} before calling {nameof(ConnectAsync)}.");
            }

            await WebSocketClient.ConnectAsync();
        }

        /// <inheritdoc />
        public async ValueTask DisconnectAsync() {
            if (!Volatile.Read(ref IsConnectedRef)) {
                throw new InvalidOperationException("Cannot disconnect when no connection is established.");
            }

            await WebSocketClient.DisconnectAsync();
        }

        /// <inheritdoc />
        public async ValueTask<SearchResponse> SearchAsync(SearchType searchType, string query) {
            if (string.IsNullOrWhiteSpace(query)) {
                throw new ArgumentNullException(nameof(query));
            }

            var path = searchType switch {
                SearchType.YouTube    => $"/loadtracks?identifier={WebUtility.UrlEncode($"scsearch:{query}")}",
                SearchType.SoundCloud => $"/loadtracks?identifier={WebUtility.UrlEncode($"ytsearch:{query}")}",
                SearchType.Direct     => $"/loadtracks?identifier={query}",
                _                     => throw new ArgumentOutOfRangeException(nameof(searchType), searchType, null)
            };

            using var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, $"{_nodeConfiguration.HttpEndpoint}{path}") {
                    Headers = {
                        {"Authorization", _nodeConfiguration.Authorization}
                    }
                };

            var searchResponse = await Extensions.HttpClient.ReadAsJsonAsync<SearchResponse>(requestMessage);
            return searchResponse;
        }

        /// <inheritdoc />
        public bool HasPlayer(ulong voiceChannelId) {
            return PlayersCache.ContainsKey(voiceChannelId);
        }

        /// <inheritdoc />
        public bool TryGetPlayer(ulong voiceChannelId, out TLavaPlayer lavaPlayer) {
            return PlayersCache.TryGetValue(voiceChannelId, out lavaPlayer);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            foreach (var (_, player) in PlayersCache) {
                await player.DisposeAsync();
            }

            await DisconnectAsync();
        }

        private ValueTask OnUserVoiceStateUpdated(VoiceState state) {
            _logger.LogDebug($"Received {state.GuildId} user voice state update.");
            if (DiscordClient.UserId != state.UserId) {
                return ValueTask.CompletedTask;
            }

            _voiceStates.TryUpdate(state.GuildId, state, default);
            return ValueTask.CompletedTask;
        }

        private ValueTask OnVoiceServerUpdated(VoiceServer server) {
            _logger.LogDebug($"Received {server.GuildId} voice server update.");
            if (_voiceStates.TryGetValue(server.GuildId, out var state))
                return WebSocketClient.SendAsync(new ServerUpdatePayload {
                    Data = new VoiceServerData(server.Token, server.Endpoint),
                    SessionId = state.SessionId,
                    GuildId = $"{server.GuildId}"
                });

            _voiceStates.TryAdd(server.GuildId, default);
            return ValueTask.CompletedTask;
        }

        private ValueTask OnOpenAsync() {
            Volatile.Write(ref IsConnectedRef, true);
            _logger.LogInformation("Connection to Lavalink established.");
            if (!_nodeConfiguration.EnableResume) {
                return ValueTask.CompletedTask;
            }

            return WebSocketClient.SendAsync(
                new ResumePayload(_nodeConfiguration.ResumeKey, _nodeConfiguration.ResumeTimeout));
        }

        private ValueTask OnCloseAsync(CloseEventArgs arg) {
            _logger.LogWarning("WebSocket connection was closed.");
            return ValueTask.CompletedTask;
        }

        private ValueTask OnErrorAsync(ErrorEventArgs arg) {
            _logger.LogCritical(arg.Exception, arg.Message);
            return ValueTask.CompletedTask;
        }

        private ValueTask OnMessageAsync(MessageEventArgs arg) {
            if (arg.Data.Length == 0) {
                _logger.LogWarning("Received empty data from WebSocket.");
                return ValueTask.CompletedTask;
            }

            _logger.LogDebug("{Message}", Encoding.UTF8.GetString(arg.Data));
            return ValueTask.CompletedTask;
        }
    }
}