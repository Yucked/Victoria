using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Victoria.Utilities;

namespace Victoria
{
    /// <summary>
    /// Manages links and nodes A.K.A websockets
    /// </summary>
    public sealed class Lavalink
    {
        private int _counter;
        private const string _prefix = "Lavalink_Node_#";
        private readonly ConcurrentDictionary<string, LavaNode> _nodes;

        /// <summary>
        /// 
        /// </summary>
        public Func<LogMessage, Task> Log;

        /// <summary>
        /// 
        /// </summary>
        public LavaNode DefaultNode => _nodes[$"{_prefix}0"];


        internal Lavalink()
        {
            _nodes = new ConcurrentDictionary<string, LavaNode>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDiscordClient"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<LavaNode> AddNodeAsync(BaseDiscordClient baseDiscordClient,
            Configuration configuration = default)
        {
            configuration = configuration.Equals(default) ? Configuration.Default : configuration;
            LogResolver.LogSeverity = configuration.Severity;
            var node_name = $"{_prefix}{_counter}";
            Interlocked.Increment(ref _counter);
            var node = new LavaNode(node_name, baseDiscordClient, configuration);
            try
            {
                await node.StartAsync().ConfigureAwait(false);
                _nodes.TryAdd($"{_prefix}{_counter}", node);
            }
            catch
            {
                Interlocked.Decrement(ref _counter);
                await node.StopAsync().ConfigureAwait(false);
                _nodes.TryRemove(node_name, out _);
            }

            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public async Task<bool> RemoveNodeAsync(string nodeName)
        {
            if (!_nodes.TryGetValue(nodeName, out var node))
                return false;
            Interlocked.Decrement(ref _counter);
            await node.StopAsync().ConfigureAwait(false);
            return _nodes.TryRemove(nodeName, out _);
        }
    }
}