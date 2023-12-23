using System;
using Victoria.WebSocket.Internal;

namespace Victoria;

/// <summary>
/// 
/// </summary>
public record Configuration {
    /// <summary>
    /// 
    /// </summary>
    public int Version { get; init; } = 4;

    /// <summary>
    /// 
    /// </summary>
    public string Hostname { get; init; } = "127.0.0.1";

    /// <summary>
    /// 
    /// </summary>
    public int Port { get; init; } = 2333;

    /// <summary>
    /// 
    /// </summary>
    public bool IsSecure { get; init; } = false;

    /// <summary>
    /// 
    /// </summary>
    public bool EnableResume { get; init; } = true;

    /// <summary>
    /// 
    /// </summary>
    public string ResumeKey { get; init; } = "Victoria";

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan ResumeTimeout { get; init; }
        = TimeSpan.FromMinutes(10);

    /// <summary>
    /// 
    /// </summary>
    public string Authorization { get; init; } = "youshallnotpass";

    /// <summary>
    ///     Whether to enable self deaf for bot.
    /// </summary>
    public bool SelfDeaf { get; set; }
        = true;

    /// <summary>
    /// 
    /// </summary>
    public WebSocketConfiguration SocketConfiguration { get; set; }
        = new() {
            ReconnectAttempts = 10,
            ReconnectDelay = 3000,
            BufferSize = 2048
        };

    internal string SocketEndpoint
        => $"{(IsSecure ? "wss" : "ws")}://{Hostname}:{Port}";

    internal string HttpEndpoint
        => $"{(IsSecure ? "https" : "http")}://{Hostname}:{Port}";
}