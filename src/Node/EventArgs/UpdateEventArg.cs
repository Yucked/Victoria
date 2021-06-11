using System;
using Victoria.Player;

namespace Victoria.Node.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct UpdateEventArg<TPlayer>
        where TPlayer : LavaPlayer {
        /// <summary>
        /// 
        /// </summary>
        public TPlayer Player { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack Track { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Position { get; internal init; }
    }
}