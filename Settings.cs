using Discord;
using System;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        /// 
        /// </summary>
        public ushort BufferSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string NodePrefix { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LogSeverity LogSeverity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ReconnectAttempts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; }

        /// <inheritdoc cref="Settings" />
        public Settings()
        {
            BufferSize = BufferSize == default ? (ushort)512 : BufferSize;
            NodePrefix ??= "Node#";
            LogSeverity = LogSeverity is default(LogSeverity) ? LogSeverity.Info : LogSeverity;
            ReconnectAttempts = ReconnectAttempts == default ? 10 : ReconnectAttempts;
            ReconnectInterval = ReconnectInterval == default ? TimeSpan.FromSeconds(10) : ReconnectInterval;
        }
    }
}