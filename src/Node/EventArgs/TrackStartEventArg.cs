using Victoria.Player;

namespace Victoria.Node.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct TrackStartEventArg<TPlayer>
        where TPlayer : LavaPlayer {
        /// <summary>
        /// 
        /// </summary>
        public TPlayer Player { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack Track { get; internal init; }
    }
}