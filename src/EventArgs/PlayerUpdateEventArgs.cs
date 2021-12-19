using System;

namespace Victoria.EventArgs {
    /// <summary>
    ///     Contains information about track position.
    /// </summary>
    public readonly struct PlayerUpdateEventArgs {
        /// <summary>
        ///     Player for which this event fired.
        /// </summary>
        public LavaPlayer Player { get; }

        /// <summary>
        ///     Track sent by Lavalink.
        /// </summary>
        public LavaTrack Track { get; }

        /// <summary>
        ///     Track's current position
        /// </summary>
        public TimeSpan? Position { get; }

        internal PlayerUpdateEventArgs(LavaPlayer player) {
            Player = player;
            Track = player?.Track;
            Position = player?.Track?.Position;
        }
    }
}