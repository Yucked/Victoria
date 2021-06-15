namespace Victoria.Player.Filters {
    /// <summary>
    /// Similar to tremolo. While tremolo oscillates the volume, vibrato oscillates the pitch.
    /// </summary>
    public struct VibratoFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public double Frequency { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public double Depth { get; init; }
    }
}