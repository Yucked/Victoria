namespace Victoria.Filters {
    /// <summary>
    /// Changes the speed, pitch, and rate. All default to 1.
    /// </summary>
    public struct TimescaleFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public double Speed { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public double Pitch { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public double Rate { get; set; }
    }
}