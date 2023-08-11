using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Victoria.Enums;
using Victoria.Interfaces;
using Victoria.Rest;
using Victoria.Rest.Lavalink;
using Victoria.Rest.Payloads;
using Victoria.Rest.Route;
using Victoria.Rest.Search;
using Victoria.WebSocket.EventArgs;
using Victoria.WebSocket.Internal;
using Victoria.WebSocket.Internal.EventArgs;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Victoria;

/// <inheritdoc />
public class LavaNode<TLavaPlayer, TLavaTrack> : IAsyncDisposable
    where TLavaTrack : ILavaTrack
    where TLavaPlayer : ILavaPlayer<TLavaTrack> {
    /// <summary>
    /// 
    /// </summary>
    public event Func<ReadyEventArg, Task> OnReady;

    /// <summary>
    /// 
    /// </summary>
    public event Func<StatsEventArg, Task> OnStats;

    /// <summary>
    /// 
    /// </summary>
    public event Func<PlayerUpdateEventArg, Task> OnPlayerUpdate;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackStartEventArg, Task> OnTrackStart;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackEndEventArg, Task> OnTrackEnd;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackExceptionEventArg, Task> OnTrackException;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackStuckEventArg, Task> OnTrackStuck;

    /// <summary>
    /// 
    /// </summary>
    public event Func<WebSocketClosedEventArg, Task> OnWebSocketClosed;

    /// <summary>
    /// 
    /// </summary>
    public string SessionId { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsConnected { get; internal set; }

    private readonly string _version;
    private readonly BaseSocketClient _baseSocketClient;
    private readonly Configuration _configuration;
    private readonly HttpClient _httpClient;
    private readonly WebSocketClient _webSocketClient;
    private readonly ILogger<LavaNode<TLavaPlayer, TLavaTrack>> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseSocketClient"></param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    public LavaNode(BaseSocketClient baseSocketClient,
                    Configuration configuration,
                    ILogger<LavaNode<TLavaPlayer, TLavaTrack>> logger) {
        _baseSocketClient = baseSocketClient;
        _configuration = configuration;
        _logger = logger;

        _webSocketClient = new WebSocketClient(configuration);
        _webSocketClient.OnOpenAsync += OnOpenAsync;
        _webSocketClient.OnErrorAsync += OnErrorAsync;
        _webSocketClient.OnCloseAsync += OnCloseAsync;
        _webSocketClient.OnDataAsync += OnDataAsync;
        _webSocketClient.OnRetryAsync += OnRetryAsync;

        _version = $"v{configuration.Version}";
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", configuration.Authorization);
        _httpClient.BaseAddress = new Uri($"{configuration.HttpEndpoint}");
    }

    /// <summary>
    ///     Starts a WebSocket connection to the specified <see cref="Configuration.Hostname" />:<see cref="Configuration.Port" />
    ///     and hooks into <see cref="BaseSocketClient" /> events.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if client is already connected.</exception>
    public async Task ConnectAsync() {
        if (IsConnected) {
            throw new InvalidOperationException(
                $"You must call {nameof(DisconnectAsync)} or {nameof(DisposeAsync)} before calling {nameof(ConnectAsync)}.");
        }

        if (_baseSocketClient.CurrentUser == null || _baseSocketClient.CurrentUser.Id == 0) {
            throw new InvalidOperationException($"{nameof(_baseSocketClient)} is not in ready state.");
        }

        _webSocketClient.AddHeader("Authorization", _configuration.Authorization);
        _webSocketClient.AddHeader("User-Id", $"{_baseSocketClient.CurrentUser.Id}");
        _webSocketClient.AddHeader("Client-Name",
            $"{nameof(Victoria)}/{typeof(Configuration).Assembly.GetName().Version}");

        await _webSocketClient.ConnectAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Disposes all players and closes websocket connection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
    public async Task DisconnectAsync() {
        if (!IsConnected) {
            throw new InvalidOperationException("Can't disconnect when client isn't connected.");
        }

        await _webSocketClient.DisconnectAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<TLavaPlayer>> GetPlayersAsync(string sessionId) {
        ArgumentNullException.ThrowIfNull(sessionId);
        var responseMessage = await _httpClient.GetAsync($"/{_version}/sessions/{sessionId}/players");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<IReadOnlyCollection<TLavaPlayer>>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="guildId"></param>
    /// <returns></returns>
    public async Task<TLavaPlayer> GetPlayerAsync(string sessionId, ulong guildId) {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(guildId);
        var responseMessage = await _httpClient.GetAsync($"/{_version}/sessions/{sessionId}/players/{guildId}");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<TLavaPlayer>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="guildId"></param>
    /// <param name="replaceTrack"></param>
    /// <param name="updatePayload"></param>
    /// <returns></returns>
    public async Task<TLavaPlayer> UpdatePlayerAsync(string sessionId,
                                                     ulong guildId,
                                                     bool replaceTrack,
                                                     UpdatePlayerPayload updatePayload) {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(guildId);
        ArgumentNullException.ThrowIfNull(replaceTrack);
        ArgumentNullException.ThrowIfNull(updatePayload);
        var responseMessage = await _httpClient.PatchAsync(
            $"/{_version}/sessions/{sessionId}/players/{guildId}?noReplace={replaceTrack}",
            new ReadOnlyMemoryContent(JsonSerializer.SerializeToUtf8Bytes(updatePayload)));
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<TLavaPlayer>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="guildId"></param>
    public async Task DestroyPlayerAsync(string sessionId, ulong guildId) {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(guildId);
        var responseMessage = await _httpClient.GetAsync($"/{_version}/sessions/{sessionId}/players/{guildId}");
        if (!responseMessage.IsSuccessStatusCode) {
            await using var stream = await responseMessage.Content.ReadAsStreamAsync();
            throw new RestException(stream);
        }

        _logger.LogInformation("Player for guild {guildId} has been destroyed.", guildId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="sessionPayload"></param>
    /// <returns></returns>
    public async Task<UpdateSessionPayload> UpdateSessionAsync(string sessionId,
                                                               UpdateSessionPayload sessionPayload) {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(sessionPayload);
        var responseMessage = await _httpClient.PatchAsync($"/{_version}/sessions/{sessionId}/",
            new ReadOnlyMemoryContent(JsonSerializer.SerializeToUtf8Bytes(sessionPayload)));
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<UpdateSessionPayload>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public async Task<SearchResponse> LoadTrackAsync(string identifier) {
        ArgumentNullException.ThrowIfNull(identifier);
        var responseMessage = await _httpClient.GetAsync($"/{_version}/loadtracks?identifier={identifier}");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<SearchResponse>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="trackHash"></param>
    /// <returns></returns>
    public async Task<TLavaTrack> DecodeTrackAsync(string trackHash) {
        ArgumentNullException.ThrowIfNull(trackHash);
        var responseMessage = await _httpClient.GetAsync($"/{_version}/decodetrack?encodedTrack={trackHash}");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<TLavaTrack>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tracksHashes"></param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<TLavaTrack>> DecodeTracksAsync(params string[] tracksHashes) {
        ArgumentNullException.ThrowIfNull(tracksHashes);
        var responseMessage = await _httpClient.PostAsync($"/{_version}/decodetrack",
            new ReadOnlyMemoryContent(JsonSerializer.SerializeToUtf8Bytes(tracksHashes)));
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<IReadOnlyCollection<TLavaTrack>>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task<LavalinkInfo> GetLavalinkInfoAsync() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/info");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<LavalinkInfo>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task<StatsEventArg> GetLavalinkStatsAsync() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/stats");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<StatsEventArg>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task<string> GetLavalinkVersion() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/routeplanner/status");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await responseMessage.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<RouteStatus> GetRoutePlannerStatusAsync() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/routeplanner/status");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        if (!responseMessage.IsSuccessStatusCode) {
            throw new RestException(stream);
        }

        return await JsonSerializer.DeserializeAsync<RouteStatus>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    public async Task UnmarkFailedAddressAsync(string address) {
        ArgumentNullException.ThrowIfNull(address);
        await _httpClient.PostAsync($"/{_version}/routeplanner/free/address",
            new StringContent(address));
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task UnmarkAllFailedAddressAsync() {
        await _httpClient.PostAsync($"/{_version}/routeplanner/free/all", default);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        await _webSocketClient.DisposeAsync()
            .ConfigureAwait(false);
        _httpClient.Dispose();
    }

    private Task OnOpenAsync() {
        IsConnected = true;

        // TODO: Handle resume?
        return Task.CompletedTask;
    }

    private Task OnCloseAsync(CloseEventArgs arg) {
        IsConnected = false;
        _logger.LogWarning("WebSocket connection closed");
        return Task.CompletedTask;
    }

    private Task OnErrorAsync(ErrorEventArgs arg) {
        _logger.LogError("{exception}, {message}", arg.Exception, arg.Message);
        return Task.CompletedTask;
    }

    private Task OnRetryAsync(RetryEventArgs arg) {
        if (arg.IsLastRetry) {
            _logger.LogError("This was the last try in establishing connection with Lavalink");
            return Task.CompletedTask;
        }

        _logger.LogWarning("Lavalink reconnect attempt #{attempts}", arg.Count);
        return Task.CompletedTask;
    }

    private async Task OnDataAsync(DataEventArgs arg) {
        if (arg.Data.Length == 0) {
            _logger.LogWarning("Didn't receive any data from websocket");
            return;
        }

        _logger.LogDebug("{data}", JsonSerializer.Serialize(arg.Data));

        try {
            var document = JsonDocument.Parse(arg.Data).RootElement;
            var guildId = document.TryGetProperty("guildId", out var idElement)
                ? ulong.Parse(idElement.GetString()!)
                : 0;
            switch (document.TryGetProperty("op", out var element) ? default : $"{element}") {
                case "ready":
                    if (OnReady == null) {
                        _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                            nameof(OnReady));
                        return;
                    }

                    SessionId = $"{document.GetProperty("sessionId")}";
                    await OnReady.Invoke(new ReadyEventArg(
                        document.GetProperty("resumed").GetBoolean(),
                        SessionId));
                    break;

                case "stats":
                    if (OnStats == null) {
                        _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                            nameof(OnStats));
                        return;
                    }

                    await OnStats.Invoke(JsonSerializer.Deserialize<StatsEventArg>(arg.Data));
                    break;

                case "playerUpdate":
                    var state = document.GetProperty("state");
                    if (OnPlayerUpdate == null) {
                        _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                            nameof(OnPlayerUpdate));
                        return;
                    }

                    await OnPlayerUpdate.Invoke(new PlayerUpdateEventArg {
                        GuildId = guildId,
                        Time = DateTimeOffset.FromUnixTimeMilliseconds(state.GetProperty("time").GetInt32()),
                        Position = TimeSpan.FromMilliseconds(state.GetProperty("position").GetInt32()),
                        IsConnected = state.GetProperty("connected").GetBoolean(),
                        Ping = state.GetProperty("ping").GetInt64()
                    });
                    break;

                case "event":
                    switch (document.GetProperty("type").GetString()) {
                        case "TrackStartEvent":
                            if (OnTrackStart == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackStart));
                                return;
                            }

                            await OnTrackStart.Invoke(new TrackStartEventArg {
                                GuildId = guildId,
                                EncodedTrack = document.GetProperty("encodedTrack").GetString()
                            });
                            break;

                        case "TrackEndEvent":
                            if (OnTrackEnd == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackEnd));
                                return;
                            }

                            await OnTrackEnd.Invoke(new TrackEndEventArg {
                                GuildId = guildId,
                                EncodedTrack = document.GetProperty("encodedTrack").GetString(),
                                Reason = document.GetProperty("reason").Deserialize<TrackEndReason>()
                            });
                            break;

                        case "TrackExceptionEvent":
                            if (OnTrackException == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackException));
                                return;
                            }

                            await OnTrackException.Invoke(new TrackExceptionEventArg {
                                GuildId = guildId,
                                EncodedTrack = document.GetProperty("encodedTrack").GetString(),
                                Exception = document.GetProperty("exception").Deserialize<TrackException>()
                            });
                            break;

                        case "TrackStuckEvent":
                            if (OnTrackStuck == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackStuck));
                                return;
                            }

                            await OnTrackStuck.Invoke(new TrackStuckEventArg {
                                GuildId = guildId,
                                EncodedTrack = document.GetProperty("EncodedTrack").GetString(),
                                Threshold = document.GetProperty("thresholdMs").GetInt64()
                            });
                            break;

                        case "WebSocketClosedEvent":
                            if (OnWebSocketClosed == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnWebSocketClosed));
                                return;
                            }

                            await OnWebSocketClosed.Invoke(new WebSocketClosedEventArg {
                                GuildId = guildId,
                                ByRemote = document.GetProperty("byRemote").GetBoolean(),
                                Code = document.GetProperty("code").GetInt32(),
                                Reason = document.GetProperty("reason").GetString()
                            });
                            break;

                        default:
                            _logger.LogError("Unknown event encountered {}. Please open an issue.",
                                document.GetProperty("type"));
                            break;
                    }

                    break;

                default: {
                    _logger.LogCritical("Unknown OP code encountered, please check lavalink implementation.");
                    break;
                }
            }
        }
        catch (Exception exception) {
            _logger.LogError(exception is JsonReaderException
                ? "Please increase buffer size in configuration."
                : $"{exception.Message} {exception}");
        }
    }
}