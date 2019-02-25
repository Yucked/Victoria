using System;
using System.Collections.Generic;
using System.Text;
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

        private Task OnShardDisconnected(Exception arg1, DiscordSocketClient arg2)
        {
            throw new NotImplementedException();
        }
    }
}
