namespace Victoria.Enums {
    /// <summary>
    /// Describes status of <see cref="LavaPlayer"/>.
    /// </summary>
    public enum PlayerState {
        /// <summary>
        /// Connected to a voice channel.
        /// </summary>
        Connected,

        /// <summary>
        /// Currently playing in connected voice channel.
        /// </summary>
        Playing,

        /// <summary>
        /// Not playing anything currently and <see cref="LavaPlayer.Track"/> set to null.
        /// </summary>
        Stopped,

        /// <summary>
        /// <see cref="LavaPlayer.Track"/> isn't null and currently paused.
        /// </summary>
        Paused,

        /// <summary>
        /// Not connected to any voice channel.
        /// </summary>
        Disconnected
    }
}