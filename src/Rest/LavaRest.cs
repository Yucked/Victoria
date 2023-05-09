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

/// <summary>
/// 
/// </summary>
/// <typeparam name="TLavaPlayer"></typeparam>
/// <typeparam name="TLavaTrack"></typeparam>
public class LavaRest<TLavaPlayer, TLavaTrack> : IAsyncDisposable
    where TLavaTrack : ILavaTrack
    where TLavaPlayer : ILavaPlayer<TLavaTrack> {
    private readonly HttpClient _httpClient;
    private readonly Configuration _configuration;
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
        _configuration = configuration;

        _httpClient.DefaultRequestHeaders.Add("Authorization", _configuration.Authorization);
        _httpClient.BaseAddress = new Uri($"{configuration.HttpEndpoint}/v3");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<TLavaPlayer>> GetPlayersAsync(string sessionId) {
        ArgumentNullException.ThrowIfNull(sessionId);
        var responseMessage = await _httpClient.GetAsync($"/sessions/{sessionId}/players");
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
        var responseMessage = await _httpClient.GetAsync($"/sessions/{sessionId}/players/{guildId}");
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
            $"/sessions/{sessionId}/players/{guildId}?noReplace={replaceTrack}",
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
        var responseMessage = await _httpClient.GetAsync($"/sessions/{sessionId}/players/{guildId}");
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
        var responseMessage = await _httpClient.PatchAsync($"/sessions/{sessionId}/",
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
        var responseMessage = await _httpClient.GetAsync($"/loadtracks?identifier={identifier}");
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
        var responseMessage = await _httpClient.GetAsync($"/decodetrack?encodedTrack={trackHash}");
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
        var responseMessage = await _httpClient.PostAsync($"/decodetrack",
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
        var responseMessage = await _httpClient.GetAsync($"/v3/info");
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
        var responseMessage = await _httpClient.GetAsync($"/stats");
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
        throw new OverflowException("ask lavalink to add versioning to this path");
        var responseMessage = await _httpClient.GetAsync($"/version");
        if (!responseMessage.IsSuccessStatusCode) {
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return default;
        }

        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<string>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<RouteStatus> GetRoutePlannerStatusAsync() {
        return default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    public async Task UnmarkFailedAddressAsync(string address) {
        ArgumentNullException.ThrowIfNull(address);
        await _httpClient.PostAsync($"/routeplanner/free/address",
            new StringContent(address));
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task UnmarkAllFailedAddressAsync() {
        await _httpClient.PostAsync($"/routeplanner/free/all", default);
    }

    public ValueTask DisposeAsync() {
        throw new NotImplementedException();
    }
}