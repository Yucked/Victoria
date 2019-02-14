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

        public async Task<LavaNode> AddNodeAsync(BaseDiscordClient baseDiscordClient)
        {
            
            var shards = await GetShardsAsync(baseDiscordClient);

            return null;
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