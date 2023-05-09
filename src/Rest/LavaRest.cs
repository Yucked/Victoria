using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;
using Victoria.Rest.Requests;
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
        _httpClient.BaseAddress = new Uri($"{configuration.HttpEndpoint}/v4");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<TLavaPlayer>> GetPlayersAsync(string sessionId) {
        ArgumentNullException.ThrowIfNull(sessionId);
        var responseMessage = await _httpClient.GetAsync($"/sessions/{sessionId}/players");
        if (!responseMessage.IsSuccessStatusCode) {
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return default;
        }

        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
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
        if (!responseMessage.IsSuccessStatusCode) {
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return default;
        }

        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<TLavaPlayer>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="replaceTrack"></param>
    /// <returns></returns>
    public async Task<TLavaPlayer> UpdatePlayerAsync(ulong guildId, bool replaceTrack = false) {
        return default;
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
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return;
        }

        _logger.LogInformation("Player for guild {guildId} has been destroyed.", guildId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="sessionRequest"></param>
    /// <returns></returns>
    public async Task<UpdateSessionRequest> UpdateSessionAsync(string sessionId,
                                                               UpdateSessionRequest sessionRequest) {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(sessionRequest);
        var responseMessage = await _httpClient.PatchAsync($"/sessions/{sessionId}/",
            new ReadOnlyMemoryContent(JsonSerializer.SerializeToUtf8Bytes(sessionRequest)));
        if (!responseMessage.IsSuccessStatusCode) {
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return default;
        }

        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<UpdateSessionRequest>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public async Task<SearchResponse> LoadTrackAsync(string identifier) {
        ArgumentNullException.ThrowIfNull(identifier);
        var responseMessage = await _httpClient.GetAsync($"/loadtracks?identifier={identifier}");
        if (!responseMessage.IsSuccessStatusCode) {
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return default;
        }

        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
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
        if (!responseMessage.IsSuccessStatusCode) {
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return default;
        }

        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
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
        if (!responseMessage.IsSuccessStatusCode) {
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return default;
        }

        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<IReadOnlyCollection<TLavaTrack>>(stream);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task GetLavalinkInfoAsync() { }

    /// <summary>
    /// 
    /// </summary>
    public async Task<StatsEventArg> GetLavalinkStatsAsync() {
        var responseMessage = await _httpClient.GetAsync($"/stats");
        if (!responseMessage.IsSuccessStatusCode) {
            _logger.LogError("{reasonPhrase}", responseMessage.ReasonPhrase);
            return default;
        }

        await using var stream = await responseMessage.Content.ReadAsStreamAsync();
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
    /// /
    /// </summary>
    public async Task UnmarkFailedAddressAsync() { }

    /// <summary>
    /// 
    /// </summary>
    public async Task UnmarkAllFailedAddressAsync() { }

    public ValueTask DisposeAsync() {
        throw new NotImplementedException();
    }
}