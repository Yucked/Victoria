namespace Victoria.Frostbyte.Enums
{
    /// <summary>
    /// Represents player state.
    /// </summary>
    public enum PlayerState
    {
        /// <summary>
        /// Connected to voice channel.
        /// </summary>
        Connected,

        /// <summary>
        /// Playing a track.
        /// </summary>
        Playing,

        /// <summary>
        /// Not playing anything.
        /// </summary>
        Stopped,

        /// <summary>
        /// Paused currently.
        /// </summary>
        Paused,

        /// <summary>
        /// Not connected to voice channel.
        /// </summary>
        Disconnected
    }
}