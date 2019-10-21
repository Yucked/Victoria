using Victoria.Enums;

namespace Victoria
{
    /// <summary>
    ///   
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Whether the next track should be played or not.
        /// </summary>
        /// <param name="trackEndReason">Track end reason given by Lavalink.</param>
        public static bool ShouldPlayNext(this TrackEndReason trackEndReason)
            => trackEndReason == TrackEndReason.Finished || trackEndReason == TrackEndReason.Failed;

        internal static bool EnsureState(this PlayerState state)
            => state == PlayerState.Connected
               || state == PlayerState.Playing
               || state == PlayerState.Paused;
    }
}