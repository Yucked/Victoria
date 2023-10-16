using System.Text.Json.Serialization;

namespace Victoria.Rest.Payloads;

/// <summary>
/// 
/// </summary>
public readonly record struct UpdatePlayerPayload(
    [property: JsonPropertyName("encodedTrack")]
    string EncodedTrack,
    [property: JsonPropertyName("identifier")]
    string Identifier,
    [property: JsonPropertyName("position")]
    int Position,
    [property: JsonPropertyName("endTime")]
    int EndTime,
    [property: JsonPropertyName("volume")]
    int Volume,
    [property: JsonPropertyName("paused")]
    int IsPaused,
    [property: JsonPropertyName("filters")]
    Filters.Filters Filters,
    [property: JsonPropertyName("voice")]
    VoiceState VoiceState
);