using System;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Victoria.Utilities;

namespace Victoria
{
    public struct Configuration
    {
        /// <summary>
        ///     Number of <see cref="BaseDiscordClient" /> shards.
        /// </summary>
        internal int Shards { get; set; }

        /// <summary>
        ///     User Id of <see cref="BaseDiscordClient" />.
        /// </summary>
        internal ulong UserId { get; set; }

        /// <summary>
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        ///     Number of reconnect attempts for websocket connection. Set to -1 for unlimited attempts.
        /// </summary>
        public int ReconnectAttempts { get; set; }

        /// <summary>
        ///     Wait time before trying again.
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; }

        /// <summary>
        ///     Websocket buffer size for receiving data.
        /// </summary>
        public ushort BufferSize { get; set; }

        /// <summary>
        ///     Websocket and Rest hostname of Lavalink server.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///     Websocket and Rest port of Lavalink server.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        ///     Lavalink server authorization.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        ///     Logging severity of everything.
        /// </summary>
        public LogSeverity Severity { get; set; }

        /// <summary>
        ///     If you want your bot to not hear anything.
        /// </summary>
        public bool SelfDeaf { get; set; }

        /// <summary>
        ///     Specify prefix for nodes.
        /// </summary>
        public string NodePrefix { get; set; }

        /// <summary>
        ///     Configure websocket resuming.
        /// </summary>
        public bool EnableResuming { get; set; }

        internal static async Task<Configuration> PrepareAsync(Configuration configuration,
            BaseDiscordClient baseClient)
        {
            async Task<int> GetShardsAsync()
            {
                switch (baseClient)
                {
                    case DiscordSocketClient client:
                        var shards = await client.GetRecommendedShardCountAsync().ConfigureAwait(false);
                        return shards;

                    case DiscordShardedClient shardedClient:
                        return shardedClient.Shards.Count;

                    default:
                        return 1;
                }
            }

            if (configuration.ReconnectAttempts is 0)
                configuration.ReconnectAttempts = 10;

            if (configuration.ReconnectInterval.TotalMilliseconds is 0)
                configuration.ReconnectInterval = TimeSpan.FromSeconds(3);

            if (configuration.BufferSize is 0)
                configuration.BufferSize = 1024;

            if (string.IsNullOrWhiteSpace(configuration.Authorization))
                configuration.Authorization = "youshallnotpass";

            if (string.IsNullOrWhiteSpace(configuration.Host))
                configuration.Host = "127.0.0.1";

            if (configuration.Port is 0)
                configuration.Port = 2333;

            if (configuration.Severity is default(LogSeverity))
                configuration.Severity = LogSeverity.Info;

            if (string.IsNullOrWhiteSpace(configuration.NodePrefix))
                configuration.NodePrefix = "Node#";

            configuration.UserId = baseClient.CurrentUser.Id;
            configuration.Shards = await GetShardsAsync().ConfigureAwait(false);

            LogResolver.LogSeverity = configuration.Severity;
            return configuration;
        }
    }
}
