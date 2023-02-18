namespace Victoria.WebSocket.Internal;

/// <summary>
/// 
/// </summary>
/// <param name="BufferSize"></param>
/// <param name="ReconnectAttempts"></param>
/// <param name="ReconnectDelay"></param>
public readonly record struct WebSocketConfiguration(
    ushort BufferSize = 512,
    int ReconnectAttempts = 10,
    int ReconnectDelay = 3000);