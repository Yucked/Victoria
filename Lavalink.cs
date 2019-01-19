using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Victoria.Utilities;

namespace Victoria
{
    /// <summary>
    ///     Manages <see cref="LavaNode" />'s.
    /// </summary>
    public sealed class Lavalink
    {
        private readonly ConcurrentDictionary<string, LavaNode> _nodes
            = new ConcurrentDictionary<string, LavaNode>();

        private int _counter;

        /// <summary>
        ///     Triggers when a node has been added, removed or moved.
        /// </summary>
        public Func<LogMessage, Task> Log;

        /// <summary>
        ///     Returns the very first node (LavaNode__#0) if any.
        /// </summary>
        public LavaNode DefaultNode
            => _nodes.FirstOrDefault().Value;

        /// <summary>
        ///     Returns the count of connected nodes.
        /// </summary>
        public int ConnectedNodes => _nodes.Count;

        /// <summary>
        ///     Adds and connects to node. If connection is successful, said node is returned.
        /// </summary>
        /// <param name="baseDiscordClient">
        ///     <see cref="BaseDiscordClient" />
        /// </param>
        /// <param name="configuration">Optional configuration. Uses default application.yml configuration.</param>
        /// <returns>
        ///     <see cref="LavaNode" />
        /// </returns>
        public async Task<LavaNode> AddNodeAsync(BaseDiscordClient baseDiscordClient,
            Configuration configuration = default)
        {
            configuration = await Configuration.PrepareAsync(configuration, baseDiscordClient).ConfigureAwait(false);
            var nodeName = $"{configuration.NodePrefix}{_counter}";

            if (_nodes.TryGetValue(nodeName, out var existingNode))
                return existingNode;

            var node = new LavaNode(nodeName, baseDiscordClient, configuration, HandleLog);

            try
            {
                await node.StartAsync().ConfigureAwait(false);
                _nodes.TryAdd(nodeName, node);
                Interlocked.Increment(ref _counter);
                Log?.Invoke(LogResolver.Info(nodeName, "Node added."));
            }
            catch
            {
                await node.StopAsync().ConfigureAwait(false);
                _nodes.TryRemove(nodeName, out _);
                Interlocked.Decrement(ref _counter);
                Log?.Invoke(LogResolver.Info(nodeName, "Node removed."));
            }

            return node;
        }

        /// <summary>
        ///     Disconnects and removes node.
        /// </summary>
        /// <param name="nodeName">Name of the node i.e: LavaNode-0.</param>
        /// <returns>
        ///     <see cref="Boolean" />
        /// </returns>
        public async Task<bool> RemoveNodeAsync(string nodeName)
        {
            if (!_nodes.TryGetValue(nodeName, out var node))
                return false;

            Interlocked.Decrement(ref _counter);

            await node.StopAsync().ConfigureAwait(false);
            node.Dispose();

            Log?.Invoke(LogResolver.Info(nodeName, "Node removed."));
            return _nodes.TryRemove(nodeName, out _);
        }

        /// <summary>
        ///     Moves a node AKA connect to a different lavalink server while preserving node's state.
        /// </summary>
        /// <param name="nodeName">Name of the node i.e: LavaNode__0.</param>
        /// <param name="configuration">File containing different configurations.</param>
        public async Task MoveNodeAsync(string nodeName, Configuration configuration)
        {
            if (!_nodes.TryGetValue(nodeName, out var node))
                return;

            await node.StopAsync().ConfigureAwait(false);
            node.Initialize(configuration);
            await node.StartAsync().ConfigureAwait(false);
            _nodes.TryUpdate(nodeName, node, node);
            Log?.Invoke(LogResolver.Info(nodeName, "Node moved."));
        }

        /// <summary>
        ///     Returns the specified node from connected nodes.
        /// </summary>
        /// <param name="nodeName">Name of the node i.e: LavaNode__0.</param>
        /// <returns></returns>
        public LavaNode GetNode(string nodeName)
        {
            return _nodes.TryGetValue(nodeName, out var node) ? node : null;
        }

        private Task HandleLog(LogMessage message)
        {
            return string.IsNullOrWhiteSpace(message.Message) && message.Exception is null
                ? Task.CompletedTask
                : Log?.Invoke(message);
        }
    }
}