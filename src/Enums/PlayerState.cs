namespace Victoria.Enums {
    /// <summary>
    /// Describes status of <see cref="LavaPlayer"/>.
    /// </summary>
    public enum PlayerState {
        /// <summary>
        /// Player isn't conencted to a voice channel
        /// </summary>
        None = 0,

        /// <summary>
        /// Currently playing a track
        /// </summary>
        Playing = 1,

        /// <summary>
        /// Not playing anything
        /// </summary>
        Stopped = 2,

        /// <summary>
        /// Playing a track but paused
        /// </summary>
        Paused = 3
    }
}