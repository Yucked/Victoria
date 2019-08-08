using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Socks.EventArgs;
using Victoria.Common.Interfaces;

namespace Victoria.Common
{
    public abstract partial class BaseClient<TPlayer, TTrack>
        where TPlayer : IPlayer<TTrack>
        where TTrack : ITrack
    {
        private async Task OnConnectedAsync()
        {
            Volatile.Write(ref RefConnected, true);
            Log(LogSeverity.Info, nameof(Common), "Websocket connection established.");
            await Task.Delay(0)
                .ConfigureAwait(false);
        }


        private async Task OnDisconnectedAsync(DisconnectEventArgs eventArgs)
        {
            Volatile.Write(ref RefConnected, false);

            await Task.Delay(0)
                .ConfigureAwait(false);
        }
    }
}
