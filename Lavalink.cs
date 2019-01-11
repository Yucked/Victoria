using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Victoria.Utilities;

namespace Victoria
{
    /// <summary>
    /// Manages <see cref="LavaNode"/>'s.
    /// </summary>
    public sealed class Lavalink
    {
        private int _counter;
        private string _prefix;
        private readonly ConcurrentDictionary<string, LavaNode> _nodes;

        /// <summary>
        /// Triggers when a node has been added, removed or moved.
        /// </summary>
        public Func<LogMessage, Task> Log;

        /// <summary>
        /// Returns the very first node (LavaNode__#0) if any.
        /// </summary>
        public LavaNode DefaultNode
            => _nodes.TryGetValue($"{_prefix}0", out var node)
                ? node
                : default;

        /// <summary>
        /// Returns the count of connected nodes.
        /// </summary>
        public int ConnectedNodes => _nodes.Count;

        /// <summary>
        /// Initialize Lavalink.
        /// </summary>
        public Lavalink()
        {
            _nodes = new ConcurrentDictionary<string, LavaNode>();
        }

        /// <summary>
        /// Adds and connects to node. If connection is successful, said node is returned. 
        /// </summary>
        /// <param name="baseDiscordClient"><see cref="BaseDiscordClient"/></param>
        /// <param name="configuration">Optional configuration. Uses default application.yml configuration.</param>
        /// <returns><see cref="LavaNode"/></returns>
        public async Task<LavaNode> AddNodeAsync(BaseDiscordClient baseDiscordClient,
            Configuration configuration = default)
        {
            configuration = Configuration.Verify(configuration);
            _prefix = configuration.NodePrefix;
            LogResolver.LogSeverity = configuration.Severity;
            var nodeName = $"{_prefix}{_counter}";
            configuration.UserId = baseDiscordClient.CurrentUser.Id;
            configuration.Shards = await GetShardsAsync(baseDiscordClient).ConfigureAwait(false);
            var node = new LavaNode(nodeName, baseDiscordClient, configuration, HandleLog);
            try
            {
                await node.StartAsync().ConfigureAwait(false);
                _nodes.TryAdd($"{_prefix}{_counter}", node);
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
        /// Disconnects and removes node.
        /// </summary>
        /// <param name="nodeName">Name of the node i.e: LavaNode-0.</param>
        /// <returns><see cref="Boolean"/></returns>
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
        /// Moves a node AKA connect to a different lavalink server while preserving node's state.
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
        /// Returns the specified node from connected nodes.
        /// </summary>
        /// <param name="nodeName">Name of the node i.e: LavaNode__0.</param>
        /// <returns></returns>
        public LavaNode GetNode(string nodeName)
            => _nodes.TryGetValue(nodeName, out var node) ? node : null;

        private Task HandleLog(LogMessage message)
            => string.IsNullOrWhiteSpace(message.Message) && message.Exception is null
                ? Task.CompletedTask
                : Log?.Invoke(message);

        private async Task<int> GetShardsAsync(BaseDiscordClient baseClient)
        {
            switch (baseClient)
            {
                case DiscordSocketClient client:
                    return await client.GetRecommendedShardCountAsync();
                case DiscordShardedClient shardedClient:
                    return shardedClient.Shards.Count;
                default: return 1;
            }
        }
    }
}