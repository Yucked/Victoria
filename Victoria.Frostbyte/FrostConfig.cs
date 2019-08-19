using System;
using Discord;
using Victoria.Common.Interfaces;

namespace Victoria.Frostbyte
{
    /// <summary>
    /// 
    /// </summary>
    public class FrostConfig : IConfig
    {
        /// <inheritdoc />
        public ushort Port { get; set; }

        /// <inheritdoc />
        public string Hostname { get; set; }

        /// <inheritdoc />
        public string Authorization { get; set; }

        /// <inheritdoc />
        public bool SelfDeaf { get; set; }

        /// <inheritdoc />
        public TimeSpan ReconnectDelay { get; set; }

        /// <inheritdoc />
        public int ReconnectAttempts { get; set; }

        /// <inheritdoc />
        public LogSeverity LogSeverity { get; set; }

        /// <inheritdoc />
        public ushort BufferSize { get; set; }
    }
}
