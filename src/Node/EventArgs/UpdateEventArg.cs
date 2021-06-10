using System;
using Victoria.Player;

namespace Victoria.Node.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct UpdateEventArg<TPlayer> where TPlayer : LavaPlayer {
        /// <summary>
        /// 
        /// </summary>
        public TPlayer Player { get; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack Track { get; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Position { get; }

        internal UpdateEventArg(TPlayer player, LavaTrack track, TimeSpan position) {
            Player = player;
            Track = track;
            Position = position;
        }
    }
}