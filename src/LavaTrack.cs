using System;
using System.Text.Json.Serialization;

namespace Victoria;

/// <summary>
/// 
/// </summary>
public class LavaTrack {
    /// <summary>
    /// 
    /// </summary>
    public string Hash { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("identifier"), JsonInclude]
    public string Id { get; private init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("title"), JsonInclude]
    public string Title { get; private init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("author"), JsonInclude]
    public string Author { get; private init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("uri"), JsonInclude]
    public string Url { get; private init; }

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan Position
        => IntPosition == 0
            ? TimeSpan.Zero
            : TimeSpan.Parse($"{IntPosition}");

    [JsonPropertyName("position"), JsonInclude]
    private int IntPosition { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan Duration
        => Length == 0
            ? TimeSpan.Zero
            : TimeSpan.Parse($"{Length}");

    [JsonPropertyName("length"), JsonInclude]
    private int Length { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isSeekable"), JsonInclude]
    public bool IsSeekable { get; private init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isStream"), JsonInclude]
    public bool IsLiveStream { get; private init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("artworkUrl"), JsonInclude]
    public string Artwork { get; private init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isrc"), JsonInclude]
    public string ISRC { get; private init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("sourceName"), JsonInclude]
    public string SourceName { get; private init; }
    
    /// <summary>
    /// 
    /// </summary>
    public object PluginInfo { get; internal set; }
}