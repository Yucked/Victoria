using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;
using Victoria.Rest.Route;
using Victoria.Rest.Search;

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
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<TLavaPlayer>> GetPlayersAsync() {
        return default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    public async Task<TLavaPlayer> GetPlayerAsync(ulong guildId) {
        return default;
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
    /// <param name="guildId"></param>
    public async Task DestroyPlayerAsync(ulong guildId) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    public async Task UpdateSessionAsync(string sessionId) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public async Task<SearchResponse> LoadTrackAsync(string identifier) {
        return default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="trackHash"></param>
    /// <returns></returns>
    public async Task<TLavaTrack> DecodeTrackAsync(string trackHash) {
        return default;
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task GetLavalinkInfoAsync() {
        
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task GetLavalinkStatsAsync() {
        
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task GetLavalinkVersion() {
        
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
    public async Task UnmarkFailedAddressAsync() {
        
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task UnmarkAllFailedAddressAsync() {
        
    }

    public ValueTask DisposeAsync() {
        throw new NotImplementedException();
    }
}