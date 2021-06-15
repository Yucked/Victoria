namespace Victoria.Player.Filters {
    /// <summary>
    /// Uses equalization to eliminate part of a band, usually targeting vocals.
    /// </summary>
    public struct KarokeFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public double Level { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public double MonoLevel { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public double FilterBand { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public double FilterWidth { get; init; }
    }
}