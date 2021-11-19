namespace Victoria.EventArgs {
    /// <summary>
    ///     Information about track that threw an exception.
    /// </summary>
    public readonly struct TrackExceptionEventArgs {
        /// <summary>
        ///     Player for which this event fired.
        /// </summary>
        public LavaPlayer Player { get; }

        /// <summary>
        ///     Track sent by Lavalink.
        /// </summary>
        public LavaTrack Track { get; }

        /// <summary>
        ///     Reason for why track threw an exception.
        /// </summary>
        public LavaException Exception { get; }

        internal TrackExceptionEventArgs(LavaPlayer player, LavaTrack lavaTrack, LavaException exception) {
            Player = player;
            Track = lavaTrack;
            Exception = exception;
        }
    }
}