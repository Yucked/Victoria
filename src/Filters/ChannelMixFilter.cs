namespace Victoria.Filters {
    /// <summary>
    /// Mixes both channels (left and right), with a configurable factor on how much each channel affects the other.
    /// With the defaults, both channels are kept independent from each other.
    /// Setting all factors to 0.5 means both channels get the same audio.
    /// </summary>
    public struct ChannelMixFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public double LeftToLeft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double LeftToRight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double RightToLeft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double RightToRight { get; set; }
    }
}