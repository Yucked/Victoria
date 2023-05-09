using System.Text.Json.Serialization;

namespace Victoria.Rest.Payloads;

/// <summary>
/// 
/// </summary>
/// <param name="ShouldResume"></param>
/// <param name="Timeout"></param>
public readonly record struct UpdateSessionPayload(
    [property: JsonPropertyName("resuming")]
    bool ShouldResume,
    [property: JsonPropertyName("timeout")]
    int Timeout);