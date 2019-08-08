using System;

namespace Victoria.Common.Interfaces
{
    /// <summary>
    ///     Represents a simple track object shared by Lavalink and Frostbyte.
    /// </summary>
    public interface ITrack : IQueueable
    {
        /// <summary>
        ///     Audio / Video track Id.
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Track's title.
        /// </summary>
        string Title { get; }

        /// <summary>
        ///     Track's length.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        ///     Track's current position.
        /// </summary>
        TimeSpan Position { get; }

        /// <summary>
        ///     Track's url.
        /// </summary>
        string Url { get; }
    }
}