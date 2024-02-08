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
    public string Id { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("title"), JsonInclude]
    public string Title { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("author"), JsonInclude]
    public string Author { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("uri"), JsonInclude]
    public string Url { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan Position
    {
        get => IntPosition == 0
            ? TimeSpan.Zero
            : TimeSpan.FromMilliseconds(IntPosition);
        set => IntPosition = (int)value.TotalMilliseconds;
    }

    [JsonPropertyName("position"), JsonInclude]
    private int IntPosition { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan Duration
    {
        get => Length == 0
            ? TimeSpan.Zero
            : TimeSpan.FromMilliseconds(Length);
        set => Length = (int)value.TotalMilliseconds;
    }

    [JsonPropertyName("length"), JsonInclude]
    private int Length { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isSeekable"), JsonInclude]
    public bool IsSeekable { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isStream"), JsonInclude]
    public bool IsLiveStream { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("artworkUrl"), JsonInclude]
    public string Artwork { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isrc"), JsonInclude]
    public string ISRC { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("sourceName"), JsonInclude]
    public string SourceName { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public object PluginInfo { get; internal set; }
}