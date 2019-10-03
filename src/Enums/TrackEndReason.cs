namespace Victoria.Enums
{
    /// <summary>
    ///     Specifies the reason for why the track ended.
    /// </summary>
    public enum TrackEndReason
    {
        /// <summary>
        ///     Track playback was completed succesfully.
        /// </summary>
        Finished,

        /// <summary>
        ///     Failed to play the track.
        /// </summary>
        Failed,

        /// <summary>
        ///     Track was stopped after receiving stop payload.
        /// </summary>
        Stopped,

        /// <summary>
        ///     Track was replaced with a new track aka skipped.
        /// </summary>
        Replaced,

        /// <summary>
        ///     Something went wrong when trying to play the track.
        /// </summary>
        Erroed
    }
}