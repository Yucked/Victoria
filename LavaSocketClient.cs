using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Victoria
{
    /// <summary>
    /// Represents a <see cref="DiscordSocketClient"/> connection to Lavalink server.
    /// </summary>
    public sealed class LavaSocketClient : LavaBaseClient
    {
        /// <summary>
        /// Starts websocket connection with Lavalink server once <see cref="DiscordSocketClient"/> hits ready event.
        /// </summary>
        /// <param name="socketClient"><see cref="DiscordSocketClient"/></param>
        /// <param name="configuration"><see cref="Configuration"/></param>
        public Task StartAsync(DiscordSocketClient socketClient, Configuration configuration = default)
        {
            socketClient.Disconnected += OnDisconnected;
            return InitializeAsync(socketClient, configuration);
        }

        private async Task OnDisconnected(Exception exception)
        {
            if (configuration.PreservePlayers)
                return;

            foreach (var player in _players.Values)
            {
                await player.DisposeAsync().ConfigureAwait(false);
            }
            _players.Clear();

            _log?.WriteLog(LogSeverity.Error, "WebSocket disconnected! Disposing all connected players.", exception);
        }
    }
}