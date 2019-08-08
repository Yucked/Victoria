using System;
using System.Threading.Tasks;
using Discord;
using Victoria.Common.Enums;

namespace Victoria.Common.Interfaces
{
    /// <summary>
    ///     Represents a simple player.
    /// </summary>
    /// <typeparam name="T">Where T implements ITrack</typeparam>
    public interface IPlayer<T> : IAsyncDisposable
        where T : ITrack
    {
        /// <summary>
        ///     Player's current voice state.
        /// </summary>
        IVoiceState VoiceState { get; }

        /// <summary>
        ///     Player's current volume.
        /// </summary>
        int Volume { get; }

        /// <summary>
        ///     Current track that is playing.
        /// </summary>
        T Track { get; }

        /// <summary>
        ///     Player's current state.
        /// </summary>
        PlayerState PlayerState { get; }

        /// <summary>
        ///     Last time player was updated.
        /// </summary>
        DateTimeOffset LastUpdate { get; }

        /// <summary>
        ///     Default queue.
        /// </summary>
        DefaultQueue<T> Queue { get; }

        /// <summary>
        ///     Voice channel this player is connected to.
        /// </summary>
        IVoiceChannel VoiceChannel { get; }

        /// <summary>
        ///     Channel
        /// </summary>
        ITextChannel TextChannel { get; }

        /// <summary>
        ///     Plays the specified track.
        /// </summary>
        /// <param name="track">An instance of <see cref="ITrack" />.</param>
        Task PlayAsync(T track);

        /// <summary>
        ///     Stops the current track if any is playing.
        /// </summary>
        Task StopAsync();

        /// <summary>
        ///     Pauses the current track if any is playing.
        /// </summary>
        Task PauseAsync();

        /// <summary>
        ///     Resumes the current track if any is playing.
        /// </summary>
        Task ResumeAsync();

        /// <summary>
        ///     Skips the current track after the specified delay.
        /// </summary>
        /// <param name="delay">If set to null, skips instantly otherwise after the specified value.</param>
        /// <returns>
        ///     <see cref="ITrack" />
        /// </returns>
        Task<T> SkipAsync(TimeSpan? delay = default);

        /// <summary>
        ///     Seeks the current track to specified position.
        /// </summary>
        /// <param name="position">Position must be less than <see cref="ITrack.Duration" />.</param>
        /// <returns></returns>
        Task SeekAsync(TimeSpan position);

        /// <summary>
        ///     Changes the current volume and updates <see cref="Volume" />.
        /// </summary>
        Task UpdateVolumeAsync(ushort volume);
    }
}
