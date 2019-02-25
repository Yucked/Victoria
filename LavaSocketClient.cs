using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LavaSocketClient : BaseLavaClient
    {
        public LavaSocketClient(DiscordSocketClient socketClient, Configuration configuration = default)
            : base(socketClient, configuration)
        {
            configuration.Shards = 1;
            socketClient.Disconnected += OnDisconnected;
        }

        public async Task OnDisconnected(Exception exception)
        {
            foreach (var player in _players.Values)
            {
                await player.DisposeAsync().ConfigureAwait(false);
            }
            _players.Clear();
        }
    }
}