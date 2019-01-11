using System;
using System.Net;
using Discord;
using Discord.Rest;

namespace Victoria
{
    public struct Configuration
    {
        /// <summary>
        /// Number of <see cref="BaseDiscordClient"/> shards.
        /// </summary>
        internal int Shards { get; set; }

        /// <summary>
        /// User Id of <see cref="BaseDiscordClient"/>.
        /// </summary>
        internal ulong UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Number of reconnect attempts for websocket connection. Set to -1 for unlimited attempts.
        /// </summary>
        public int ReconnectAttempts { get; set; }

        /// <summary>
        /// Wait time before trying again.
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; }

        /// <summary>
        /// Websocket buffer size for receiving data.
        /// </summary>
        public ushort BufferSize { get; set; }

        /// <summary>
        /// Websocket and Rest hostname of Lavalink server.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Websocket and Rest port of Lavalink server.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Lavalink server authorization.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Logging severity of everything.
        /// </summary>
        public LogSeverity Severity { get; set; }

        /// <summary>
        /// If you want your bot to not hear anything.
        /// </summary>
        public bool SelfDeaf { get; set; }

        /// <summary>
        /// Specify prefix for nodes.
        /// </summary>
        public string NodePrefix { get; set; }

        /// <summary>
        /// Configure websocket resuming.
        /// </summary>
        public bool EnableResuming { get; set; }

        /// <summary>
        /// Default configuration.
        /// </summary>
        private static Configuration Default
            => new Configuration
            {
                ReconnectAttempts = 10,
                ReconnectInterval = TimeSpan.FromSeconds(3),
                BufferSize = 1024,
                Authorization = "youshallnotpass",
                Host = "127.0.0.1",
                Port = 2333,
                Severity = LogSeverity.Verbose,
                SelfDeaf = true,
                NodePrefix = "LavaNode-",
                Proxy = default
            };

        internal static Configuration Verify(Configuration configuration)
        {
            if (configuration.ReconnectAttempts is 0)
                configuration.ReconnectAttempts = Default.ReconnectAttempts;

            if (configuration.ReconnectInterval.TotalMilliseconds is 0)
                configuration.ReconnectInterval = Default.ReconnectInterval;

            if (configuration.BufferSize is 0)
                configuration.BufferSize = Default.BufferSize;

            if (string.IsNullOrWhiteSpace(configuration.Authorization))
                configuration.Authorization = Default.Authorization;

            if (string.IsNullOrWhiteSpace(configuration.Host))
                configuration.Host = Default.Host;

            if (configuration.Port is 0)
                configuration.Port = Default.Port;

            if (configuration.Severity is default(LogSeverity))
                configuration.Severity = Default.Severity;

            if (string.IsNullOrWhiteSpace(configuration.NodePrefix))
                configuration.NodePrefix = Default.NodePrefix;

            return configuration;
        }
    }
}