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
        public LavalinkSettings LavalinkSettings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LavaNodeSettings LavaNodeSettings { get; set; }

        /// <inheritdoc cref="Settings" />
        public Settings()
        {
            LavalinkSettings ??= new LavalinkSettings();
            LavaNodeSettings ??= new LavaNodeSettings();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class LavalinkSettings
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

        /// <inheritdoc cref="LavalinkSettings" />
        public LavalinkSettings()
        {
            BufferSize = BufferSize == default ? (ushort)512 : BufferSize;
            NodePrefix ??= "Node#";
            LogSeverity = LogSeverity is default(LogSeverity) ? LogSeverity.Info : LogSeverity;
            ReconnectAttempts = ReconnectAttempts == default ? 10 : ReconnectAttempts;
            ReconnectInterval = ReconnectInterval == default ? TimeSpan.FromSeconds(10) : ReconnectInterval;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class LavaNodeSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Authorization { get; set; }

        /// <inheritdoc cref="LavaNodeSettings" />
        public LavaNodeSettings()
        {
            Host ??= "127.0.0.1";
            Port = Port is 0 ? (ushort)2333 : Port;
            Authorization ??= "youshallnotpass";
        }
    }
}