using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Victoria
{
    public sealed class LavaShardClient : BaseLavaClient
    {
        public LavaShardClient(DiscordShardedClient shardedClient, Configuration configuration = default)
            : base(shardedClient, configuration)
        {
            configuration.Shards = shardedClient.Shards.Count;
            shardedClient.ShardDisconnected += OnShardDisconnected;
        }

        private async Task OnShardDisconnected(Exception exception, DiscordSocketClient socketClient)
        {
            foreach (var guild in socketClient.Guilds)
            {
                if (!_players.TryRemove(guild.Id, out var player))
                    continue;

                await player.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
