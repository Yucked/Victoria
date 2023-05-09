using System.Text.Json.Serialization;

namespace Victoria.Rest;

/// <summary>
/// 
/// </summary>
/// <param name="Token">Discord voice token to authenticate with</param>
/// <param name="Endpoint">Discord voice endpoint to connect to</param>
/// <param name="SessionId">Discord voice session id to authenticate with</param>
/// <param name="IsConnected">Whether the player is connected</param>
/// <param name="Ping">Roundtrip latency in milliseconds to the voice gateway (-1 if not connected).</param>
public readonly record struct VoiceState(
    [property: JsonPropertyName("token")]
    string Token,
    [property: JsonPropertyName("endpoint")]
    string Endpoint,
    [property: JsonPropertyName("sessionId")]
    string SessionId,
    [property: JsonPropertyName("connected")]
    bool IsConnected,
    [property: JsonPropertyName("ping")]
    int Ping);