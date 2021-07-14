namespace Victoria.Filters {
    /// <summary>
    /// Higher frequencies get suppressed, while lower frequencies pass through this filter, thus the name low pass.
    /// </summary>
    public struct LowPassFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public double Smoothing { get; set; }
    }
}