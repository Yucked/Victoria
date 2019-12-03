using System;
using Discord;

namespace Victoria {
    /// <summary>
    /// </summary>
    public sealed class LavaConfig {
        /// <summary>
        /// </summary>
        public bool EnableResume { get; set; } = false;

        /// <summary>
        /// </summary>
        public TimeSpan ResumeTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// </summary>
        public string ResumeKey { get; set; } = nameof(Victoria);

        /// <summary>
        ///     Port to connect to.
        /// </summary>
        public ushort Port { get; set; } = 2333;

        /// <summary>
        ///     Server's IP/Hostname.
        /// </summary>
        public string Hostname { get; set; } = "127.0.0.1";

        /// <summary>
        ///     Server's password/authentication.
        /// </summary>
        public string Authorization { get; set; } = "youshallnotpass";

        /// <summary>
        ///     Whether to enable self deaf for bot.
        /// </summary>
        public bool SelfDeaf { get; set; } = true;

        /// <summary>
        ///     Reconnection delay for retrying websocket connection.
        /// </summary>
        public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        ///     How many reconnect attempts are allowed.
        /// </summary>
        public int ReconnectAttempts { get; set; } = 10;

        /// <summary>
        ///     Log serverity for logging everything.
        /// </summary>
        public LogSeverity LogSeverity { get; set; } = LogSeverity.Debug;

        /// <summary>
        ///     Max buffer size for receiving websocket message.
        /// </summary>
        public ushort BufferSize { get; set; } = 512;
    }
}