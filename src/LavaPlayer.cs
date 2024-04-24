using System;
using System.Text.Json.Serialization;
using Victoria.Converters;
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
    [JsonPropertyName("track"), JsonInclude, JsonConverter(typeof(LavaTrackConverter))]
    public LavaTrack Track { get; internal init; }

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
    public VoiceState VoiceState { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("state"), JsonInclude]
    public PlayerState State { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    /// TODO: How to handle queue?
    [Obsolete("Use LavaPlayerExtensions.Queue or LavaPlayer#GetQueue on LavaPlayer instance")]
    public LavaQueue<TLavaTrack> Queue { get; }
}