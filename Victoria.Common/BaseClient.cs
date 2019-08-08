using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Socks;
using Victoria.Common.Interfaces;

namespace Victoria.Common
{
    /// <inheritdoc />
    public abstract partial class BaseClient<TPlayer, TTrack> : IClient<TPlayer, TTrack>
        where TPlayer : IPlayer<TTrack>
        where TTrack : ITrack
    {
        /// <inheritdoc />
        public event Func<LogMessage, Task> OnLog;

        /// <inheritdoc />
        public bool IsConnected
            => Volatile.Read(ref RefConnected);

        /// <inheritdoc />
        public IEnumerable<TPlayer> Players
            => PlayerCache.Values;
        
        /// <summary>
        /// 
        /// </summary>
        protected readonly BaseSocketClient SocketClient;

        /// <summary>
        /// 
        /// </summary>
        protected ConcurrentDictionary<ulong, TPlayer> PlayerCache;

        /// <summary>
        /// 
        /// </summary>
        protected bool RefConnected;

        /// <summary>
        /// 
        /// </summary>
        protected ClientSock Sock;

        private readonly IConfig _config;

        /// <summary>
        /// </summary>
        /// <param name="socketClient">An instance of <see cref="DiscordSocketClient" /> or <see cref="DiscordShardedClient" />.</param>
        /// <param name="config">An instance of <see cref="IConfig" />.</param>
        protected BaseClient(BaseSocketClient socketClient, IConfig config)
        {
            Ensure.NotNull(socketClient, config, config.Hostname, config.Port);
            SocketClient = socketClient;
            _config = config;

            Sock = new ClientSock(new Endpoint(config.Hostname, config.Port, false), config.BufferSize);
            Sock.OnConnected += OnConnectedAsync;
            Sock.OnDisconnected += OnDisconnectedAsync;

            PlayerCache = new ConcurrentDictionary<ulong, TPlayer>();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Throws when <see cref="BaseSocketClient" /> returns CurrentUser as null.</exception>
        public virtual async Task ConnectAsync()
        {
            if (Volatile.Read(ref RefConnected))
                Throw.InvalidOperation(
                    $"You must call {nameof(DisconnectAsync)} or {nameof(DisposeAsync)} before calling {nameof(ConnectAsync)}.");

            if (SocketClient?.CurrentUser == null || SocketClient.CurrentUser.Id == 0)
                Throw.InvalidOperation($"{nameof(SocketClient)} is not in ready state.");

            await Sock.ConnectAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task DisconnectAsync()
        {
            if (!Volatile.Read(ref RefConnected))
                Throw.InvalidOperation("Can't disconnect when client isn't connected.");

            foreach (var (_, value) in PlayerCache)
                await value.DisposeAsync()
                    .ConfigureAwait(false);

            PlayerCache.Clear();

            await Sock.DisconnectAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public abstract Task<TPlayer> JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = default);

        /// <inheritdoc />
        public virtual Task MoveAsync(IVoiceChannel voiceChannel)
        {
            Throw.NotImplemented();
            return Task.Delay(0);
        }

        /// <inheritdoc />
        public virtual async Task LeaveAsync(IVoiceChannel voiceChannel)
        {
            if (!Volatile.Read(ref RefConnected))
                Throw.InvalidOperation("Can't execute this operation when client isn't connected.");

            if (!PlayerCache.TryGetValue(voiceChannel.GuildId, out var player))
                return;

            await player.DisposeAsync()
                .ConfigureAwait(false);

            PlayerCache.TryRemove(voiceChannel.GuildId, out _);
        }

        /// <inheritdoc />
        public virtual bool HasPlayer(IGuild guild)
            => PlayerCache.ContainsKey(guild.Id);

        /// <inheritdoc />
        public virtual bool TryGetPlayer(IGuild guild, out TPlayer player)
            => PlayerCache.TryGetValue(guild.Id, out player);

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync()
                .ConfigureAwait(false);

            await Sock.DisposeAsync()
                .ConfigureAwait(false);

            PlayerCache.Clear();

            Sock = null;
            PlayerCache = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected void Log(LogSeverity severity, string source, string message, Exception exception = null)
        {
            if (severity > _config.LogSeverity)
                return;

            var logMessage = new LogMessage(severity, source, message, exception);
            OnLog?.Invoke(logMessage)
                .ConfigureAwait(false);
        }
    }
}
