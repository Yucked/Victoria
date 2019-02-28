using Discord;
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

        private async Task OnDisconnected(Exception exception)
        {
            foreach (var player in _players.Values)
            {
                await player.DisposeAsync().ConfigureAwait(false);
            }
            _players.Clear();

            _log?.Invoke(VictoriaExtensions.LogMessage(LogSeverity.Error, "WebSocket disconnected! Disposing all connected players.", exception));
        }
    }
}