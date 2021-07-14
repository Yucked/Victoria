namespace Victoria.Filters {
    /// <summary>
    /// Uses equalization to eliminate part of a band, usually targeting vocals.
    /// </summary>
    public struct KarokeFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public double Level { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double MonoLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double FilterBand { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double FilterWidth { get; set; }
    }
}