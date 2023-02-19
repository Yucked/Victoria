using System.Text.Json.Serialization;

namespace Victoria.Rest;

/// <summary>
/// 
/// </summary>
/// <param name="Token"></param>
/// <param name="Endpoint"></param>
/// <param name="SessionId"></param>
/// <param name="IsConnected"></param>
/// <param name="Ping"></param>
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