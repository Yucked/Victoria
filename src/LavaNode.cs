using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Victoria.Enums;
using Victoria.Rest;
using Victoria.Rest.Lavalink;
using Victoria.Rest.Payloads;
using Victoria.Rest.Route;
using Victoria.Rest.Search;
using Victoria.WebSocket.EventArgs;
using Victoria.WebSocket.Internal;
using Victoria.WebSocket.Internal.EventArgs;

namespace Victoria;

/// <inheritdoc />
public class LavaNode : LavaNode<LavaPlayer<LavaTrack>, LavaTrack> {
    /// <inheritdoc />
    public LavaNode(DiscordSocketClient discordSocketClient,
                    Configuration configuration,
                    ILogger<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>> logger)
        : base(discordSocketClient, configuration, logger) { }
    
    /// <inheritdoc />
    public LavaNode(DiscordShardedClient discordShardedClient,
                    Configuration configuration,
                    ILogger<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>> logger)
        : base(discordShardedClient, configuration, logger) { }
}

/// <inheritdoc />
public class LavaNode<TLavaPlayer, TLavaTrack> : IAsyncDisposable
    where TLavaTrack : LavaTrack
    where TLavaPlayer : LavaPlayer<TLavaTrack> {
    /// <summary>
    ///     Fired when the WebSocket connection to Lavalink is established and the session is ready.
    /// </summary>
    public event Func<ReadyEventArg, Task> OnReady;

    /// <summary>
    ///     Fired periodically by Lavalink with CPU, memory, and player statistics.
    /// </summary>
    public event Func<StatsEventArg, Task> OnStats;

    /// <summary>
    ///     Fired when Lavalink sends a position update for an active player.
    /// </summary>
    public event Func<PlayerUpdateEventArg, Task> OnPlayerUpdate;

    /// <summary>
    ///     Fired when a track begins playing on a player.
    /// </summary>
    public event Func<TrackStartEventArg, Task> OnTrackStart;

    /// <summary>
    ///     Fired when a track finishes, either naturally or because it was stopped.
    /// </summary>
    public event Func<TrackEndEventArg, Task> OnTrackEnd;

    /// <summary>
    ///     Fired when Lavalink encounters an exception while playing a track.
    /// </summary>
    public event Func<TrackExceptionEventArg, Task> OnTrackException;

    /// <summary>
    ///     Fired when a track is stuck and is not making progress for longer than the threshold.
    /// </summary>
    public event Func<TrackStuckEventArg, Task> OnTrackStuck;

    /// <summary>
    ///     Fired when the voice WebSocket connection between Lavalink and Discord is closed.
    /// </summary>
    public event Func<WebSocketClosedEventArg, Task> OnWebSocketClosed;

    /// <summary>
    ///     The Lavalink session ID assigned after a successful connection. Used in all REST requests.
    /// </summary>
    public string SessionId { get; internal set; }

    /// <summary>
    ///     <see langword="true" /> when the WebSocket connection to Lavalink is open.
    /// </summary>
    public bool IsConnected { get; internal set; }
    
    private static readonly HttpClient _httpClient = new();
    private static readonly string _clientName =
        $"{nameof(Victoria)}/{typeof(Configuration).Assembly.GetName().Version}";

    private readonly string _version;
    private readonly BaseSocketClient _baseSocketClient;
    private readonly Configuration _configuration;
    private readonly WebSocketClient _webSocketClient;
    private readonly ILogger<LavaNode<TLavaPlayer, TLavaTrack>> _logger;
    private readonly ConcurrentDictionary<ulong, VoiceState> _voiceStates;
    
    private LavaNode(BaseSocketClient baseSocketClient,
                     Configuration configuration,
                     ILogger<LavaNode<TLavaPlayer, TLavaTrack>> logger) {
        _configuration = configuration;
        _logger = logger;
        
        _baseSocketClient = baseSocketClient;
        _baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
        _baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdatedAsync;
        
        _webSocketClient = new WebSocketClient(configuration);
        _webSocketClient.OnOpenAsync += OnOpenAsync;
        _webSocketClient.OnErrorAsync += OnErrorAsync;
        _webSocketClient.OnCloseAsync += OnCloseAsync;
        _webSocketClient.OnDataAsync += OnDataAsync;
        _webSocketClient.OnRetryAsync += OnRetryAsync;
        
        _version = $"v{configuration.Version}";
        if (_httpClient.BaseAddress == null) {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", configuration.Authorization);
            _httpClient.BaseAddress = new Uri($"{configuration.HttpEndpoint}");
        }

        _voiceStates = new ConcurrentDictionary<ulong, VoiceState>();
    }
    
    /// <summary>
    ///     Initialises a new <see cref="LavaNode{TLavaPlayer,TLavaTrack}" /> using a <see cref="DiscordSocketClient" />.
    /// </summary>
    /// <param name="discordSocketClient">The Discord socket client used to receive voice state and server updates.</param>
    /// <param name="configuration">Connection and behaviour settings for the Lavalink node.</param>
    /// <param name="logger">Logger instance provided by the host's DI container.</param>
    public LavaNode(DiscordSocketClient discordSocketClient,
                    Configuration configuration,
                    ILogger<LavaNode<TLavaPlayer, TLavaTrack>> logger)
        : this(discordSocketClient as BaseSocketClient, configuration, logger) { }

    /// <summary>
    ///     Initialises a new <see cref="LavaNode{TLavaPlayer,TLavaTrack}" /> using a <see cref="DiscordShardedClient" />.
    /// </summary>
    /// <param name="discordShardedClient">The sharded Discord client used to receive voice state and server updates.</param>
    /// <param name="configuration">Connection and behaviour settings for the Lavalink node.</param>
    /// <param name="logger">Logger instance provided by the host's DI container.</param>
    public LavaNode(DiscordShardedClient discordShardedClient,
                    Configuration configuration,
                    ILogger<LavaNode<TLavaPlayer, TLavaTrack>> logger)
        : this(discordShardedClient as BaseSocketClient, configuration, logger) { }
    
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
        _webSocketClient.AddHeader("Client-Name", _clientName);
        
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
    ///     Joins the specified voice channel, creating a player for the guild if one does not already exist.
    /// </summary>
    /// <param name="voiceChannel">The voice channel to join.</param>
    /// <returns>The <typeparamref name="TLavaPlayer" /> associated with the channel's guild.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="ConnectAsync" /> has not been called first.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="voiceChannel" /> is <see langword="null" />.</exception>
    public async Task<TLavaPlayer> JoinAsync(IVoiceChannel voiceChannel) {
        if (!IsConnected) {
            throw new InvalidOperationException(
                $"You must call {nameof(ConnectAsync)} before joining a voice channel.");
        }
        
        ArgumentNullException.ThrowIfNull(voiceChannel);
        
        var player = await UpdatePlayerAsync(voiceChannel.GuildId);
        var user = await voiceChannel.GetUserAsync(_baseSocketClient.CurrentUser.Id);
        if (user != null) {
            return player;
        }
        
        LavaPlayerExtensions.Queue.TryAdd(voiceChannel.GuildId, new LavaQueue<LavaTrack>());
        await voiceChannel.ConnectAsync(_configuration.SelfDeaf, false, true);
        
        return player;
    }
    
    /// <summary>
    ///     Leaves the specified channel only if <typeparamref name="TLavaPlayer" /> is connected to it.
    /// </summary>
    /// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
    /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
    public async Task LeaveAsync(IVoiceChannel voiceChannel) {
        if (!IsConnected) {
            throw new InvalidOperationException("Can't execute this operation when websocket isn't connected.");
        }
        
        ArgumentNullException.ThrowIfNull(voiceChannel);
        await voiceChannel.DisconnectAsync()
            .ConfigureAwait(false);
        LavaPlayerExtensions.Queue?.TryRemove(voiceChannel.GuildId, out _);
        await DestroyPlayerAsync(voiceChannel.GuildId);
    }
    
    /// <summary>
    ///     Returns all active players for the current session from the Lavalink server.
    /// </summary>
    /// <returns>A read-only collection of <typeparamref name="TLavaPlayer" /> instances.</returns>
    public async Task<IReadOnlyCollection<TLavaPlayer>> GetPlayersAsync() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/sessions/{SessionId}/players");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<IReadOnlyCollection<TLavaPlayer>>(stream);
    }
    
    /// <summary>
    ///     Returns the player for the specified guild from the Lavalink server.
    /// </summary>
    /// <param name="guildId">The Discord guild ID whose player should be retrieved.</param>
    /// <returns>The <typeparamref name="TLavaPlayer" /> for the guild.</returns>
    public async Task<TLavaPlayer> GetPlayerAsync(ulong guildId) {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/sessions/{SessionId}/players/{guildId}");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<TLavaPlayer>(stream);
    }
    
    /// <summary>
    ///     Sends an update to the Lavalink player for the specified guild (track, volume, filters, voice state, etc.).
    /// </summary>
    /// <param name="guildId">The Discord guild ID of the player to update.</param>
    /// <param name="replaceTrack">
    ///     When <see langword="false" /> (default), the new track replaces any currently playing track immediately.
    ///     When <see langword="true" />, the update is ignored if a track is already playing.
    /// </param>
    /// <param name="updatePayload">The payload describing the properties to change on the player.</param>
    /// <returns>The updated <typeparamref name="TLavaPlayer" />.</returns>
    public async Task<TLavaPlayer> UpdatePlayerAsync(ulong guildId,
                                                     bool replaceTrack = false,
                                                     UpdatePlayerPayload updatePayload = default) {
        var responseMessage = await _httpClient.PatchAsync(
            $"/{_version}/sessions/{SessionId}/players/{guildId}?noReplace={replaceTrack}",
            updatePayload.AsContent());
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<TLavaPlayer>(stream);
    }
    
    /// <summary>
    ///     Destroys the Lavalink player for the specified guild, stopping any playback.
    /// </summary>
    /// <param name="guildId">The Discord guild ID of the player to destroy.</param>
    public async Task DestroyPlayerAsync(ulong guildId) {
        var responseMessage = await _httpClient.DeleteAsync($"/{_version}/sessions/{SessionId}/players/{guildId}");
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode,
            await responseMessage.Content.ReadAsStreamAsync());
        _logger.LogInformation("Player for guild {guildId} has been destroyed.", guildId);
    }
    
    /// <summary>
    ///     Updates the current Lavalink session configuration (e.g. resume settings).
    /// </summary>
    /// <param name="sessionPayload">The new session configuration to apply.</param>
    /// <returns>The updated <see cref="UpdateSessionPayload" /> returned by the server.</returns>
    public async Task<UpdateSessionPayload> UpdateSessionAsync(
        UpdateSessionPayload sessionPayload) {
        ArgumentNullException.ThrowIfNull(sessionPayload);
        var responseMessage = await _httpClient.PatchAsync($"/{_version}/sessions/{SessionId}/",
            new ReadOnlyMemoryContent(JsonSerializer.SerializeToUtf8Bytes(sessionPayload)));
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<UpdateSessionPayload>(stream);
    }
    
    /// <summary>
    ///     Asks Lavalink to resolve an identifier (URL, search query, or direct track ID) and returns the result.
    /// </summary>
    /// <param name="identifier">
    ///     The track identifier to load — a direct URL, a Lavalink search prefix such as <c>ytsearch:query</c>,
    ///     or a raw track ID.
    /// </param>
    /// <returns>A <see cref="SearchResponse" /> describing the load result and any resolved tracks.</returns>
    public async Task<SearchResponse> LoadTrackAsync(string identifier) {
        ArgumentNullException.ThrowIfNull(identifier);
        var responseMessage = await _httpClient.GetAsync($"/{_version}/loadtracks?identifier={identifier}");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return new SearchResponse(await JsonDocument.ParseAsync(stream));
    }
    
    /// <summary>
    ///     Decodes a single Base64-encoded Lavalink track hash back into a <typeparamref name="TLavaTrack" />.
    /// </summary>
    /// <param name="trackHash">The Base64-encoded track hash to decode.</param>
    /// <returns>The decoded <typeparamref name="TLavaTrack" />.</returns>
    public async Task<TLavaTrack> DecodeTrackAsync(string trackHash) {
        ArgumentNullException.ThrowIfNull(trackHash);
        var responseMessage =
            await _httpClient.GetAsync($"/{_version}/decodetrack?encodedTrack={HttpUtility.UrlEncode(trackHash)}");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<TLavaTrack>(stream, Extensions.Options);
    }
    
    /// <summary>
    ///     Decodes multiple Base64-encoded Lavalink track hashes in a single request.
    /// </summary>
    /// <param name="tracksHashes">One or more Base64-encoded track hashes to decode.</param>
    /// <returns>A read-only collection of decoded <typeparamref name="TLavaTrack" /> instances.</returns>
    public async Task<IReadOnlyCollection<TLavaTrack>> DecodeTracksAsync(params string[] tracksHashes) {
        ArgumentNullException.ThrowIfNull(tracksHashes);
        var responseMessage = await _httpClient.PostAsync($"/{_version}/decodetracks",
            new ReadOnlyMemoryContent(JsonSerializer.SerializeToUtf8Bytes(tracksHashes)));
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<IReadOnlyCollection<TLavaTrack>>(stream, Extensions.Options);
    }
    
    /// <summary>
    ///     Returns version, source manager, and plugin information from the connected Lavalink server.
    /// </summary>
    public async Task<LavalinkInfo> GetLavalinkInfoAsync() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/info");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<LavalinkInfo>(stream);
    }
    
    /// <summary>
    ///     Returns the current resource usage statistics from the Lavalink server (CPU, memory, players).
    /// </summary>
    public async Task<StatsEventArg> GetLavalinkStatsAsync() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/stats");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<StatsEventArg>(stream);
    }
    
    /// <summary>
    ///     Returns the raw version string reported by the Lavalink server.
    /// </summary>
    public async Task<string> GetLavalinkVersionAsync() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/routeplanner/status");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await responseMessage.Content.ReadAsStringAsync();
    }
    
    /// <summary>
    ///     Returns the current route planner status, including any failed addresses.
    /// </summary>
    /// <returns>A <see cref="RouteStatus" /> describing the active route planner configuration.</returns>
    public async Task<RouteStatus> GetRoutePlannerStatusAsync() {
        var responseMessage = await _httpClient.GetAsync($"/{_version}/routeplanner/status");
        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        RestException.ThrowIfNot200(responseMessage.IsSuccessStatusCode, stream);
        return await JsonSerializer.DeserializeAsync<RouteStatus>(stream);
    }
    
    /// <summary>
    ///     Removes a single IP address from the route planner's failed-address list.
    /// </summary>
    /// <param name="address">The IP address to unmark.</param>
    public async Task UnmarkFailedAddressAsync(string address) {
        ArgumentNullException.ThrowIfNull(address);
        await _httpClient.PostAsync($"/{_version}/routeplanner/free/address",
            new StringContent(address));
    }
    
    /// <summary>
    ///     Removes all IP addresses from the route planner's failed-address list.
    /// </summary>
    public async Task UnmarkAllFailedAddressAsync() {
        await _httpClient.PostAsync($"/{_version}/routeplanner/free/all", default);
    }
    
    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        await _webSocketClient.DisposeAsync()
            .ConfigureAwait(false);
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
        
        _logger.LogDebug("{data}", Encoding.UTF8.GetString(arg.Data));
        
        try {
            var document = JsonDocument.Parse(arg.Data).RootElement;
            var guildId = document.TryGetProperty("guildId", out var idElement)
                ? ulong.Parse(idElement.GetString()!)
                : 0;
            
            switch (document.GetProperty("op").GetString()) {
                case "ready":
                    SessionId = $"{document.GetProperty("sessionId")}";
                    if (OnReady == null) {
                        _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                            nameof(OnReady));
                        return;
                    }
                    
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
                        Time = DateTimeOffset.FromUnixTimeMilliseconds(state.GetProperty("time").GetInt64()),
                        Position = TimeSpan.FromMilliseconds(state.GetProperty("position").GetInt64()),
                        IsConnected = state.GetProperty("connected").GetBoolean(),
                        Ping = state.GetProperty("ping").GetInt64()
                    });
                    break;
                
                case "event":
                    LavaTrack track = default;
                    if (document.TryGetProperty("track", out var trackElement)) {
                        track = trackElement.Deserialize<TLavaTrack>(Extensions.Options);
                    }
                    
                    switch (document.GetProperty("type").GetString()) {
                        case "TrackStartEvent":
                            if (OnTrackStart == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackStart));
                                return;
                            }
                            
                            await OnTrackStart.Invoke(new TrackStartEventArg {
                                GuildId = guildId,
                                Track = track
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
                                Track = track,
                                Reason = Enum.Parse<TrackEndReason>(document.GetProperty("reason").GetString()!, true)
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
                                Track = track,
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
                                Track = track,
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
                                Reason = document.GetProperty("reason").GetString()!
                            });
                            break;
                        
                        default:
                            _logger.LogError("Unknown event encountered {}. Please open an issue.",
                                document.GetProperty("type"));
                            break;
                    }
                    
                    break;
                
                default: {
                    _logger.LogCritical("Unknown OP code encountered {}, please check lavalink implementation.",
                        document.GetProperty("op"));
                    break;
                }
            }
        }
        catch (Exception exception) {
            _logger.LogError(exception is JsonException
                ? $"There was a problem parsing JSON! You may need to increase the buffer size in Configuration.  Error Message: {exception.Message}"
                : $"{exception.Message} {exception}");
        }
    }
    
    private async Task OnUserVoiceStateUpdatedAsync(SocketUser user,
                                                    SocketVoiceState pastState,
                                                    SocketVoiceState currentState) {
        if (_baseSocketClient.CurrentUser?.Id != user.Id) {
            return;
        }
        
        var guildId = (currentState.VoiceChannel ?? pastState.VoiceChannel).Guild.Id;
        if (_voiceStates.TryGetValue(guildId, out var voiceState)) {
            voiceState.SessionId = currentState.VoiceSessionId;
            await UpdatePlayerAsync(guildId, updatePayload: new UpdatePlayerPayload(VoiceState: voiceState));
        }
        
        voiceState = new VoiceState(null, null, currentState.VoiceSessionId, $"{currentState.VoiceChannel.Id}");
        _voiceStates[guildId] = voiceState;
    }
    
    private Task OnVoiceServerUpdatedAsync(SocketVoiceServer voiceServer) {
        if (!_voiceStates.TryGetValue(voiceServer.Guild.Id, out var voiceState)) {
            voiceState = new VoiceState(voiceServer.Token, voiceServer.Endpoint, string.Empty, string.Empty);
            _voiceStates.TryAdd(voiceServer.Guild.Id, voiceState);
            return Task.CompletedTask;
        }
        
        voiceState.Token = voiceServer.Token;
        voiceState.Endpoint = voiceServer.Endpoint;
        _voiceStates[voiceServer.Guild.Id] = voiceState;
        
        return UpdatePlayerAsync(voiceServer.Guild.Id, updatePayload: new UpdatePlayerPayload(VoiceState: voiceState));
    }
}