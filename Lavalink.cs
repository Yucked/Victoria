using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Victoria.Misc;

namespace Victoria
{
    public sealed class Lavalink
    {
        private readonly List<(Endpoint endpoint, LavaNode node)>
            _nodes = new List<(Endpoint endpoint, LavaNode node)>();

        private LavaConfig Config;

        /// <summary>
        /// Number of connected nodes.
        /// </summary>
        public int _nodesCount => _nodes.Count;

        /// <summary>
        /// Global logging of everything basically.
        /// </summary>
        public event AsyncEvent<LogMessage> Log;

        /// <summary>
        /// Returns the first Lavanode if any otherwise null.
        /// </summary>
        public LavaNode DefaultNode => _nodes[0].node;

        /// <summary>
        /// Fires up websocket and tries to connect to Lavalink server.
        /// </summary>
        /// <param name="client"><see cref="BaseDiscordClient"/></param>
        /// <param name="config"><see cref="LavaConfig"/></param>
        /// <returns><see cref="LavaNode"/></returns>
        public async Task<LavaNode> ConnectAsync(BaseDiscordClient client, LavaConfig config = default)
        {
            Config = config.Equals(default(LavaConfig)) ? LavaConfig.Default : config;
            var existing = _nodes.FirstOrDefault(x => x.endpoint.Equals(Config.Endpoint));
            if (existing.node != null)
                return existing.node;

            Config.UserId = client.CurrentUser.Id;
            Config.Shards = await GetShardsAsync(client);
            var socket = new LavaSocket(Config, this);
            var node = new LavaNode(client, socket, Config);
            try
            {
                await node.StartAsync();
                _nodes.Add((Config.Endpoint, node));
            }
            catch
            {
                node.Dispose();
                socket.Dispose();
                _nodes.Remove((Config.Endpoint, node));
            }

            return node;
        }

        /// <summary>
        /// Get a LavaNode for the specified endpoint.
        /// </summary>
        /// <param name="endpoint"><see cref="Endpoint"/></param>
        /// <returns><see cref="LavaNode"/></returns>
        public LavaNode GetNode(Endpoint endpoint)
        {
            return _nodes.FirstOrDefault(x => x.endpoint.Equals(endpoint)).node;
        }

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

        internal void LogDebug(string message)
        {
            switch (Config.Severity)
            {
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    var logMessage = new LogMessage(Config.Severity, "Victoria", message);
                    Log?.Invoke(logMessage);
                    break;
            }
        }

        internal void LogInfo(string message)
        {
            switch (Config.Severity)
            {
                case LogSeverity.Info:
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    var logMessage = new LogMessage(Config.Severity, "Victoria", message);
                    Log?.Invoke(logMessage);
                    break;
            }
        }

        internal void LogError(string message, Exception exc = null)
        {
            switch (Config.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                case LogSeverity.Warning:
                case LogSeverity.Error:
                    var logMessage = new LogMessage(Config.Severity, "Victoria", message, exc);
                    Log?.Invoke(logMessage);
                    break;
            }
        }
    }
}