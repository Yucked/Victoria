using System.Text.Json.Serialization;

namespace Victoria.Rest.Requests;

/// <summary>
/// 
/// </summary>
/// <param name="ShouldResume"></param>
/// <param name="Timeout"></param>
public readonly record struct UpdateSessionRequest(
    [property: JsonPropertyName("resuming")]
    bool ShouldResume,
    [property: JsonPropertyName("timeout")]
    int Timeout);