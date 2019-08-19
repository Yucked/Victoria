namespace Victoria.Frostbyte.EventArgs
{
    /// <summary>
    /// </summary>
    public sealed class TrackErrorEventArgs
    {
        /// <summary>
        ///     Inner exception message.
        /// </summary>
        public string Reason { get; private set; }
    }
}