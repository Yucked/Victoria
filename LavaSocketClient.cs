using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LavaSocketClient : LavaBaseClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task StartAsync(DiscordSocketClient socketClient, Configuration configuration = default)
        {
            configuration ??= new Configuration
            {
                UserId = socketClient.CurrentUser.Id,
                Shards = 1
            };

            socketClient.Disconnected += OnDisconnected;
            return InitializeAsync(socketClient, configuration);
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