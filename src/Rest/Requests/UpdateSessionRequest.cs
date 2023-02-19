using System.Text.Json.Serialization;

namespace Victoria.Rest.Requests;

public readonly record struct UpdateSessionRequest(
    [property: JsonPropertyName("resuming")]
    bool ShouldResume,
    [property: JsonPropertyName("timeout")]
    int Timeout);