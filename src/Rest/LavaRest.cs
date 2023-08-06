using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;
using Victoria.Rest.Lavalink;
using Victoria.Rest.Payloads;
using Victoria.Rest.Route;
using Victoria.Rest.Search;
using Victoria.WebSocket.EventArgs;

namespace Victoria.Rest;

/// <inheritdoc />
public class LavaRest : LavaRest<LavaPlayer, LavaTrack> {
    /// <inheritdoc />
    public LavaRest(HttpClient httpClient, Configuration configuration, ILogger<LavaRest> logger)
        : base(httpClient, configuration, logger) { }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TLavaPlayer"></typeparam>
/// <typeparam name="TLavaTrack"></typeparam>
public class LavaRest<TLavaPlayer, TLavaTrack> : IAsyncDisposable
    where TLavaTrack : ILavaTrack
    where TLavaPlayer : ILavaPlayer<TLavaTrack> {
    private readonly string _version;
    private readonly HttpClient _httpClient;
    private readonly ILogger<LavaRest<TLavaPlayer, TLavaTrack>> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    public LavaRest(HttpClient httpClient,
                    Configuration configuration,
                    ILogger<LavaRest<TLavaPlayer, TLavaTrack>> logger) {
        _logger = logger;
        _httpClient = httpClient;

        _version = $"v{configuration.Version}";
        _httpClient.DefaultRequestHeaders.Add("Authorization", configuration.Authorization);
        _httpClient.BaseAddress = new Uri($"{configuration.HttpEndpoint}");
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
    public ValueTask DisposeAsync() {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }
}