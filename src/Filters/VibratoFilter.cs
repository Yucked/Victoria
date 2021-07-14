namespace Victoria.Filters {
    /// <summary>
    /// Similar to tremolo. While tremolo oscillates the volume, vibrato oscillates the pitch.
    /// </summary>
    public struct VibratoFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Depth { get; set; }
    }
}