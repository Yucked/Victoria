using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Entities.Enums;
using Victoria.Entities.Responses;
using Victoria.Helpers;

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

        private Task OnDisconnected(Exception arg)
        {
            throw new NotImplementedException();
        }
    }
}