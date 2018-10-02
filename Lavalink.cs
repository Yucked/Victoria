using Discord.Rest;
using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Victoria
{
    public sealed class Lavalink
    {
        private ConcurrentDictionary<Endpoint, LavaNode> Nodes
            => new ConcurrentDictionary<Endpoint, LavaNode>();

        /// <summary>
        ///  Connect to a Lavalink Node.
        /// </summary>
        /// <param name="client"><see cref="DiscordSocketClient"/> <see cref="DiscordShardedClient"/></param>
        /// <param name="config"><see cref="LavaConfig"/></param>
        /// <returns><see cref="LavaNode"/></returns>
        public async Task<LavaNode> ConnectAsync(BaseDiscordClient client, LavaConfig config = default)
        {
            if (Nodes.ContainsKey(config.Socket))
                return Nodes[config.Socket];
            config = config.Equals(default(LavaConfig)) ? LavaConfig.Default : config;
            var node = new LavaNode(client, config);
            Nodes.TryAdd(config.Socket, node);
            try
            {
                await node.StartAsync().ConfigureAwait(false);
            }
            catch
            {
                Nodes.TryRemove(config.Socket, out _);
                throw;
            }

            return node;
        }

        /// <summary>
        /// Get a LavaNode for a specific endpoint.
        /// </summary>
        /// <param name="endpoint">Socked Endpoint</param>
        /// <returns><see cref="LavaNode"/></returns>
        public LavaNode GetNode(Endpoint endpoint)
            => Nodes.ContainsKey(endpoint) ? Nodes[endpoint] : null;

        /// <summary>
        /// Nodes Count.
        /// </summary>
        public int NodesCount => Nodes.Count;

        /// <summary>
        /// Return the default LavaNode if any.
        /// </summary>
        public LavaNode DefaultNode => Nodes.FirstOrDefault().Value;
    }
}