using System;
using Victoria.WebSocket.Internal;

namespace Victoria;

/// <summary>
///     Holds all configuration options for connecting Victoria to a Lavalink server.
/// </summary>
public record Configuration {
    /// <summary>
    ///     Lavalink REST/WebSocket API version to target. Defaults to <c>4</c>.
    /// </summary>
    public int Version { get; set; } = 4;

    /// <summary>
    ///     Hostname or IP address of the Lavalink server. Defaults to <c>127.0.0.1</c>.
    /// </summary>
    public string Hostname { get; set; } = "127.0.0.1";

    /// <summary>
    ///     Port the Lavalink server is listening on. Defaults to <c>2333</c>.
    /// </summary>
    public int Port { get; set; } = 2333;

    /// <summary>
    ///     When <see langword="true" />, connects over <c>wss://</c> and <c>https://</c> instead of plain WebSocket/HTTP.
    /// </summary>
    public bool IsSecure { get; set; } = false;

    /// <summary>
    ///     When <see langword="true" />, Victoria will attempt to resume an existing Lavalink session after reconnecting.
    /// </summary>
    public bool EnableResume { get; set; } = true;

    /// <summary>
    ///     Key sent to Lavalink when resuming a session. Defaults to <c>"Victoria"</c>.
    /// </summary>
    public string ResumeKey { get; set; } = "Victoria";

    /// <summary>
    ///     How long Lavalink will wait before destroying a session whose client has disconnected.
    ///     Defaults to <c>10 minutes</c>.
    /// </summary>
    public TimeSpan ResumeTimeout { get; set; }
        = TimeSpan.FromMinutes(10);

    /// <summary>
    ///     Password used to authenticate with the Lavalink server. Defaults to <c>"youshallnotpass"</c>.
    /// </summary>
    public string Authorization { get; set; } = "youshallnotpass";

    /// <summary>
    ///     Whether to enable self deaf for bot.
    /// </summary>
    public bool SelfDeaf { get; set; }
        = true;

    /// <summary>
    ///     Low-level WebSocket tuning options such as buffer size, reconnect attempts, and reconnect delay.
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