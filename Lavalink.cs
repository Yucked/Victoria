using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Victoria.Misc;

namespace Victoria
{
    public sealed class Lavalink
    {
        private ConcurrentDictionary<Endpoint, LavaNode> Nodes
            => new ConcurrentDictionary<Endpoint, LavaNode>();

        private LavaConfig Config;

        /// <summary>
        ///     Nodes Count.
        /// </summary>
        public int NodesCount => Nodes.Count;

        /// <summary>
        ///     Logging
        /// </summary>
        public event AsyncEvent<LogMessage> Log;

        /// <summary>
        /// Returns the first Lavanode if any otherwise null.
        /// </summary>
        public LavaNode DefaultNode => Nodes.FirstOrDefault().Value;

        /// <summary>
        ///     Connect to a Lavalink Node.
        /// </summary>
        /// <param name="client">
        ///     <see cref="DiscordSocketClient" /> <see cref="DiscordShardedClient" />
        /// </param>
        /// <param name="config">
        ///     <see cref="LavaConfig" />
        /// </param>
        /// <returns>
        ///     <see cref="LavaNode" />
        /// </returns>
        public async Task<LavaNode> ConnectAsync(BaseDiscordClient client, LavaConfig config = default)
        {
            if (Nodes.ContainsKey(config.Endpoint))
                return Nodes[config.Endpoint];
            Config = config.Equals(default(LavaConfig)) ? LavaConfig.Default : config;
            Config.UserId = client.CurrentUser.Id;
            Config.Shards = await GetShardsAsync(client);
            var socket = new LavaSocket(Config, this);
            var node = new LavaNode(client, socket, Config);
            Nodes.TryAdd(Config.Endpoint, node);
            try
            {
                await node.StartAsync();
            }
            catch
            {
                node.Dispose();
                socket.Dispose();
                Nodes.TryRemove(config.Endpoint, out _);
            }

            return node;
        }

        /// <summary>
        ///     Get a LavaNode for a specific endpoint.
        /// </summary>
        /// <param name="endpoint">Socked Endpoint</param>
        /// <returns>
        ///     <see cref="LavaNode" />
        /// </returns>
        public LavaNode GetNode(Endpoint endpoint)
        {
            return Nodes.ContainsKey(endpoint) ? Nodes[endpoint] : null;
        }

        private async Task<int> GetShardsAsync(BaseDiscordClient baseClient)
        {
            switch (baseClient)
            {
                case DiscordSocketClient client:
                    return await client.GetRecommendedShardCountAsync();
                case DiscordShardedClient shardedClient:
                    return shardedClient.Shards.Count;
                default: return 1;
            }
        }

        internal void LogDebug(string message)
        {
            switch (Config.Severity)
            {
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    var logMessage = new LogMessage(Config.Severity, "Victoria", message);
                    Log?.Invoke(logMessage);
                    break;
            }
        }

        internal void LogInfo(string message)
        {
            switch (Config.Severity)
            {
                case LogSeverity.Info:
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    var logMessage = new LogMessage(Config.Severity, "Victoria", message);
                    Log?.Invoke(logMessage);
                    break;
            }
        }

        internal void LogError(string message, Exception exc = null)
        {
            switch (Config.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                case LogSeverity.Warning:
                case LogSeverity.Error:
                    var logMessage = new LogMessage(Config.Severity, "Victoria", message, exc);
                    Log?.Invoke(logMessage);
                    break;
            }
        }
    }
}