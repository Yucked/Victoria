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
    public int Version { get; set; } = 4;

    /// <summary>
    /// 
    /// </summary>
    public string Hostname { get; set; } = "127.0.0.1";

    /// <summary>
    /// 
    /// </summary>
    public int Port { get; set; } = 2333;

    /// <summary>
    /// 
    /// </summary>
    public bool IsSecure { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public bool EnableResume { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public string ResumeKey { get; set; } = "Victoria";

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan ResumeTimeout { get; set; }
        = TimeSpan.FromMinutes(10);

    /// <summary>
    /// 
    /// </summary>
    public string Authorization { get; set; } = "youshallnotpass";

    /// <summary>
    ///     Whether to enable self deaf for bot.
    /// </summary>
    public bool SelfDeaf { get; set; }
        = true;

    /// <summary>
    ///     Whether to preserve the queue of a guild after the bot has left the channel (note that the queue will only be preserved until lavalink is restarted).
    /// </summary>
    public bool PreserveQueue { get; set; }
        = false;

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