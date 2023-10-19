using System.Text.Json.Serialization;
using Victoria.Rest;
using Victoria.Rest.Filters;

namespace Victoria;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TLavaTrack"></typeparam>
public class LavaPlayer<TLavaTrack>
    where TLavaTrack : LavaTrack {
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("guildId"), JsonInclude, JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public ulong GuildId { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    /// TODO: API doesn't return it this way
    //[JsonPropertyName("guildId"), JsonInclude]
    public LavaTrack Track { get; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("volume"), JsonInclude]
    public int Volume { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("paused"), JsonInclude]
    public bool IsPaused { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("filters"), JsonInclude]
    public Filters Filters { get; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("voice"), JsonInclude]
    public VoiceState VoiceState { get; }

    /// <summary>
    /// 
    /// </summary>
    /// TODO: How to handle queue?
    public LavaQueue<TLavaTrack> Queue { get; }
    
    // TODO: Need property for player state
    // https://github.com/lavalink-devs/Lavalink/blob/master/IMPLEMENTATION.md#player-state
}