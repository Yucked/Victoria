using System;
using Discord;
using Victoria.Common.Interfaces;

namespace Victoria.Lavalink
{
    /// <summary>
    /// </summary>
    public class LavaConfig : IConfig
    {
        /// <summary>
        /// </summary>
        public bool EnableResume { get; set; } = false;

        /// <summary>
        /// </summary>
        public TimeSpan ResumeTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// </summary>
        public string ResumeKey { get; set; } = nameof(Victoria);

        /// <inheritdoc />
        public ushort Port { get; set; } = 2333;

        /// <inheritdoc />
        public string Hostname { get; set; } = "127.0.0.1";

        /// <inheritdoc />
        public string Authorization { get; set; } = "youshallnotpass";

        /// <inheritdoc />
        public bool SelfDeaf { get; set; } = true;

        /// <inheritdoc />
        public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <inheritdoc />
        public int ReconnectAttempts { get; set; } = 10;

        /// <inheritdoc />
        public LogSeverity LogSeverity { get; set; } = LogSeverity.Debug;

        /// <inheritdoc />
        public ushort BufferSize { get; set; } = 512;
    }
}
