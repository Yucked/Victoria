using Discord;
using System;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        /// 
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ushort? BufferSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? SelfDeaf { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LogSeverity? LogSeverity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ReconnectAttempts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal ulong UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal int Shards { get; set; }

        internal static LogSeverity InternalSeverity;

        public Configuration()
        {
            Host ??= "127.0.0.1";
            Port = Port is 0 ? 2333 : Port;
            Password ??= "youshallnotpass";
            SelfDeaf ??= true;
            BufferSize ??= 512;
            LogSeverity ??= Discord.LogSeverity.Info;
            ReconnectAttempts = ReconnectAttempts == default ? 10 : ReconnectAttempts;
            ReconnectInterval = ReconnectInterval == default ? TimeSpan.FromSeconds(10) : ReconnectInterval;
            InternalSeverity = LogSeverity.Value;
        }
    }
}