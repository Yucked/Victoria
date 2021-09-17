using Victoria.Player;

namespace Victoria.Node.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct TrackStuckEventArg<TLavaPlayer, TLavaTrack>
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        /// <summary>
        /// 
        /// </summary>
        public TLavaPlayer Player { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack Track { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public long Threshold { get; internal init; }
    }
}