using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Lavalink : IAsyncDisposable
    {
        private int NodeNum;
        private readonly ConcurrentDictionary<int, LavaNode> _nodes;
        private readonly Settings _settings;

        /// <summary>
        /// 
        /// </summary>
        public event Func<LogMessage, Task> Log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public Lavalink(Settings settings = null)
        {
            _settings = settings ?? new Settings();
            _nodes = new ConcurrentDictionary<int, LavaNode>();
        }

        public async Task<LavaNode> AddNodeAsync(DiscordSocketClient socketClient,
            LavaNodeSettings settings)
        {
            var hash = socketClient.GetHashCode();
            if (_nodes.TryGetValue(hash, out LavaNode node))
                return node;

            settings.Shards = settings.Shards is null ?
                await socketClient.GetRecommendedShardCountAsync().ConfigureAwait(false)
                : 1;

            node = new LavaNode(settings);
            await node.InitializeAsync().ConfigureAwait(false);
            return node;
        }

        public async Task<IEnumerable<LavaNode>> AddNodesAsync(DiscordShardedClient shardedClient,
            LavaNodeSettings settings)
        {
            settings.With(shardedClient.Shards.Count, shardedClient.CurrentUser.Id);
            var add = shardedClient.Shards.Select(x => AddNodeAsync(x, settings));
            var nodes = await Task.WhenAll(add).ConfigureAwait(false);
            return nodes;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var node in _nodes.Values)
                await node.DisposeAsync().ConfigureAwait(false);

            _nodes.Clear();
            GC.SuppressFinalize(this);
        }
    }
}