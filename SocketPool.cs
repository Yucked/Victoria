using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SocketPool
    {
        private int? shards;
        private readonly ConcurrentDictionary<int, LavaSocket> _connections;

        /// <summary>
        /// 
        /// </summary>
        public int TotalConnections
            => _connections.Count;

        public SocketPool(int shards)
        {
            this.shards = shards;
            _connections = new ConcurrentDictionary<int, LavaSocket>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task AddConnectionAsync()
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