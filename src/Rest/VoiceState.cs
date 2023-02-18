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
    string Token,
    string Endpoint,
    string SessionId,
    bool IsConnected,
    int Ping);