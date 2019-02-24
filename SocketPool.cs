using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Victoria.Configs;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SocketPool
    {
        private readonly ConcurrentDictionary<int, LavaSocket> _connections;

        /// <summary>
        /// 
        /// </summary>
        public int TotalConnections
            => _connections.Count;

        public SocketPool()
        {
            _connections = new ConcurrentDictionary<int, LavaSocket>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task AddConnectionAsync(BaseSocketClient baseSocketClient, EndpointConfig endpoint = null)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task RemoveConnectionAsync()
        {

        }
    }
}