using System.Text.Json.Serialization;

namespace Victoria.Rest;

/// <summary>
/// 
/// </summary>
/// <param name="Token">Discord voice token to authenticate with</param>
/// <param name="Endpoint">Discord voice endpoint to connect to</param>
/// <param name="SessionId">Discord voice session id to authenticate with</param>
public record struct VoiceState(
    [property: JsonPropertyName("token")]
    string Token,
    [property: JsonPropertyName("endpoint")]
    string Endpoint,
    [property: JsonPropertyName("sessionId")]
    string SessionId);