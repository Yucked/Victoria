using Discord;
using System;

namespace Victoria
{
    /// <summary>
    /// Configuration follows application.yml. Values should match up in configuration and application.yml.
    /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        /// Lavalink host.
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        /// Lavalink port.
        /// </summary>
        public int Port { get; set; } = 2333;

        /// <summary>
        /// Lavalink password.
        /// </summary>
        public string Password { get; set; } = "youshallnotpass";

        /// <summary>
        /// Websocket buffer size.
        /// </summary>
        public ushort? BufferSize { get; set; } = 512;

        /// <summary>
        /// Self deaf client.
        /// </summary>
        public bool SelfDeaf { get; set; } = true;

        /// <summary>
        /// Logging severity.
        /// </summary>
        public LogSeverity LogSeverity { get; set; } = LogSeverity.Info;

        /// <summary>
        /// Websocket reconnect attempts.
        /// </summary>
        public int ReconnectAttempts { get; set; } = 10;

        /// <summary>
        /// Websocket reconnect delay.
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Whether players should be preserved if Lavalink is closed.
        /// </summary>
        public bool ShouldPreservePlayers { get; set; } = false;

        internal int Shards { get; set; }
        internal ulong UserId { get; set; }
        internal static LogSeverity InternalSeverity { get; set; }

        public Configuration SetInternals(ulong userId, int shards)
        {
            UserId = userId;
            Shards = shards;
            InternalSeverity = LogSeverity;
            return this;
        }
    }
}