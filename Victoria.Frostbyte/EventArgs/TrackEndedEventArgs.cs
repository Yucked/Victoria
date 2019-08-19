using Victoria.Frostbyte.Enums;

namespace Victoria.Frostbyte.EventArgs
{
    /// <summary>
    /// </summary>
    public sealed class TrackEndedEventArgs
    {
        /// <summary>
        ///     Reason for why track ended.
        /// </summary>
        public TrackEndReason Reason { get; private set; }
    }
}
