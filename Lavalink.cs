using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Victoria
{
    public sealed class Lavalink : IAsyncDisposable
    {
        private int NodeNum;
        private readonly ConcurrentDictionary<int, LavaNode> _nodes;
        private readonly Settings _settings;

        public Lavalink(Settings settings = null)
        {
            _settings = settings ?? new Settings();

            _nodes = new ConcurrentDictionary<int, LavaNode>();
        }

        public async Task<LavaNode> AddNodeAsync(BaseDiscordClient baseDiscordClient, LavaNodeSettings nodeSettings = default)
        {
            var node = default(LavaNode);
            nodeSettings ??= _settings.LavaNodeSettings;
            int hashCode;

            switch (baseDiscordClient)
            {
                case DiscordSocketClient socketClient:
                    hashCode = socketClient.GetHashCode();

                    if (_nodes.TryGetValue(hashCode, out node))
                        break;

                    node = new LavaNode(nodeSettings);
                    _nodes.TryAdd(hashCode, node);
                    Interlocked.Increment(ref NodeNum);
                    break;


                case DiscordShardedClient shardedClient:

                    foreach (var shard in shardedClient.Shards)
                    {
                        hashCode = shard.GetHashCode();
                        if (_nodes.TryGetValue(hashCode, out node))
                            continue;

                        node = new LavaNode(nodeSettings);
                        _nodes.TryAdd(hashCode, node);
                        Interlocked.Increment(ref NodeNum);
                    }

                    break;
            }

            return node;
        }

        private async ValueTask<int> GetShardsAsync(BaseDiscordClient baseDiscordClient)
        {
            return baseDiscordClient switch
            {
                DiscordSocketClient socketClient => (await socketClient.GetRecommendedShardCountAsync()),
                DiscordShardedClient shardedClient => shardedClient.Shards.Count,
                _ => 1
            };
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