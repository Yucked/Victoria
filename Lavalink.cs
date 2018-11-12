using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Victoria
{
    /// <summary>
    /// Manages links and nodes A.K.A websockets
    /// </summary>
    public sealed class Lavalink
    {
        public Func<LogMessage, Task> Log;
        private readonly HashSet<Sockeon> _nodes;

        public Lavalink()
        {
            _nodes = new HashSet<Sockeon>();
        }

        public async Task AddNodeAsync()
        {
            var sockeon = new Sockeon();

            await sockeon.ConnectAsync();
            _nodes.Add(sockeon);
        }


        public async Task RemoveNodeAsync(int index)
        {
        }
    }
}