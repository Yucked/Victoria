using Victoria.Player;

namespace Victoria.Node.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct TrackStartEventArg<TPlayer> where TPlayer : LavaPlayer {
        /// <summary>
        /// 
        /// </summary>
        public TPlayer Player { get; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack Track { get; }

        internal TrackStartEventArg(TPlayer player, LavaTrack track) {
            Player = player;
            Track = track;
        }
    }
}