using System;
using Victoria.WebSocket;

namespace Victoria.Node {
    /// <summary>
    /// Configuration for <see cref="LavaNode"/>
    /// </summary>
    public sealed class NodeConfiguration {
        /// <summary>
        /// </summary>
        public bool EnableResume { get; set; }
            = false;

        /// <summary>
        ///     Server's IP/Hostname.
        /// </summary>
        public string Hostname { get; set; }
            = "127.0.0.1";

        /// <summary>
        ///     Port to connect to.
        /// </summary>
        public ushort Port { get; set; }
            = 2333;

        /// <summary>
        ///     Server's password/authentication.
        /// </summary>
        public string Authorization { get; set; }
            = "youshallnotpass";

        /// <summary>
        ///     Use Secure Socket Layer (SSL) security protocol when connecting to Lavalink.
        /// </summary>
        public bool IsSecure { get; init; }
            = false;

        /// <summary>
        ///     Applies User-Agent header to all requests.
        /// </summary>
        public string UserAgent { get; set; }
            = null;

        /// <summary>
        /// </summary>
        public string ResumeKey { get; set; }
            = nameof(Victoria);

        /// <summary>
        /// </summary>
        public TimeSpan ResumeTimeout { get; set; }
            = TimeSpan.FromSeconds(30);

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
                ReconnectDelay = TimeSpan.FromSeconds(3),
                BufferSize = 512
            };

        internal string HttpEndpoint
            => (IsSecure ? "https" : "http") + Endpoint;

        internal string Endpoint
            => $"://{Hostname}:{Port}";
    }
}