using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Victoria.Common.Interfaces
{
    /// <summary>
    ///     Provides a basic interface for interacting with server.
    /// </summary>
    /// <typeparam name="TPlayer">Type constraint of <see cref="IPlayer{T}" /></typeparam>
    /// <typeparam name="TTrack">Type constraint of <see cref="ITrack" /> </typeparam>
    public interface IClient<TPlayer, TTrack> : IAsyncDisposable
        where TPlayer : IPlayer<TTrack>
        where TTrack : ITrack
    {
        /// <summary>
        ///     Fires whenever a log message is sent.
        /// </summary>
        event Func<LogMessage, Task> OnLog;

        /// <summary>
        ///     Checks if the client has an active WebSocket connection.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     Collection of <typeparamref name="TPlayer" />.
        /// </summary>
        IEnumerable<TPlayer> Players { get; }

        /// <summary>
        ///     Starts a WebSocket connection to the specified <see cref="IConfig.Hostname" />:<see cref="IConfig.Port" />
        ///     and hooks into <see cref="BaseSocketClient" /> events.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if client is already connected.</exception>
        Task ConnectAsync();

        /// <summary>
        ///     Disposes all players and closes websocket connection.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
        Task DisconnectAsync();

        /// <summary>
        ///     Joins the specified voice channel and returns the connected <typeparamref name="TPlayer" />.
        /// </summary>
        /// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
        /// <param name="textChannel">An instance of <see cref="ITextChannel" />.</param>
        /// <returns>
        ///     <typeparamref name="TPlayer" />
        /// </returns>
        Task<TPlayer> JoinAsync(IVoiceChannel voiceChannel, ITextChannel textChannel = default);

        /// <summary>
        ///     Moves from one voice channel to another.
        /// </summary>
        /// <param name="voiceChannel">Voice channel to connect to.</param>
        /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
        Task MoveAsync(IVoiceChannel voiceChannel);

        /// <summary>
        ///     Leaves the specified channel only if <typeparamref name="TPlayer" /> is connected to it.
        /// </summary>
        /// <param name="voiceChannel">An instance of <see cref="IVoiceChannel" />.</param>
        /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
        Task LeaveAsync(IVoiceChannel voiceChannel);

        /// <summary>
        ///     Checks if <typeparamref name="TPlayer" /> exists for specified guild.
        /// </summary>
        /// <param name="guild">An instance of <see cref="IGuild" />.</param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        bool HasPlayer(IGuild guild);

        /// <summary>
        ///     Returns either an existing or null player.
        /// </summary>
        /// <param name="guild">An instance of <see cref="IGuild" />.</param>
        /// <param name="player">An instance of <typeparamref name="TPlayer" /></param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        bool TryGetPlayer(IGuild guild, out TPlayer player);
    }
}