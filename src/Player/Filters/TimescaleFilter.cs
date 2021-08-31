namespace Victoria.Player.Filters {
    /// <summary>
    /// Changes the speed, pitch, and rate. All default to 1.
    /// </summary>
    public struct TimescaleFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public double Speed { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public double Pitch { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public double Rate { get; init; }
    }
}