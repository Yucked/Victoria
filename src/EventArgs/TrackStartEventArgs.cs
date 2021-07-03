namespace Victoria.EventArgs {
    /// <summary>
    ///     Information about the track that started.
    /// </summary>
    public readonly struct TrackStartEventArgs {
        /// <summary>
        ///     Player for which this event fired.
        /// </summary>
        public LavaPlayer Player { get; }

        /// <summary>
        ///     Track sent by Lavalink.
        /// </summary>
        public LavaTrack Track { get; }

        internal TrackStartEventArgs(LavaPlayer player, LavaTrack track) {
            Player = player;
            Track = track;
        }
    }
}