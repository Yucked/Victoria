using System.Text.Json.Serialization;

namespace Victoria.Rest.Payloads;

/// <summary>
/// 
/// </summary>
public readonly record struct UpdatePlayerPayload(
    [property: JsonPropertyName("encodedTrack")]
    string EncodedTrack = default,
    [property: JsonPropertyName("identifier")]
    string Identifier = default,
    [property: JsonPropertyName("position")]
    int Position = default,
    [property: JsonPropertyName("endTime")]
    int EndTime = default,
    [property: JsonPropertyName("volume")]
    int Volume = default,
    [property: JsonPropertyName("paused")]
    bool IsPaused = default,
    [property: JsonPropertyName("filters")]
    Filters.Filters Filters = default,
    [property: JsonPropertyName("voice")]
    VoiceState VoiceState = default
);