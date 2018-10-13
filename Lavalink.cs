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
        ///     Return the default LavaNode if any.
        /// </summary>
        public LavaNode DefaultNode => Nodes.FirstOrDefault().Value;

        /// <summary>
        ///     Logging
        /// </summary>
        public event AsyncEvent<LogMessage> Log; 

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
            if (Nodes.ContainsKey(config.Socket))
                return Nodes[config.Socket];
            Config = config.Equals(default(LavaConfig)) ? LavaConfig.Default : config;
            var shards = await GetShardsAsync(client);
            var socket = new LavaSocket(Config, this, shards, client.CurrentUser.Id);
            var node = new LavaNode(client, socket, Config, this);
            Nodes.TryAdd(Config.Socket, node);
            try
            {
                node.Start();
            }
            catch
            {
                Nodes.TryRemove(Config.Socket, out var lavaNode);
                await lavaNode.StopAsync().ConfigureAwait(false);
                throw;
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

        internal void InvokeLog(LogSeverity severity, string message, Exception exc = null)
        {
            var logMessage = new LogMessage(severity, "Victoria", message, exc);
            Log?.Invoke(logMessage);
        }
    }
}