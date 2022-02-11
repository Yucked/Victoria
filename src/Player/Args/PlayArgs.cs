using System;

namespace Victoria.Player.Args {
    /// <summary>
    /// Arguments for <see cref="LavaPlayer.PlayAsync"/>
    /// </summary>
    public struct PlayArgs<TLavaTrack>
        where TLavaTrack : LavaTrack {
        /// <summary>
        /// Which track to play, <see cref="LavaTrack"/>
        /// </summary>
        public TLavaTrack Track { get; set; }

        /// <summary>
        /// Whether to replace the track. Returns <see cref="TrackEndReason.Replaced"/> when used.
        /// </summary>
        public bool NoReplace { get; set; }

        /// <summary>
        /// Set the volume of the player when playing <see cref="Track"/>.
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// Whether to pause the player when <see cref="Track"/> is ready to play.
        /// </summary>
        public bool ShouldPause { get; set; }

        /// <summary>
        /// Start time of <see cref="Track"/>.
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// End time of <see cref="Track"/>.
        /// </summary>
        public TimeSpan? EndTime { get; set; }
    }
}