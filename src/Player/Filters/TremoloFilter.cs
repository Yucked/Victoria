namespace Victoria.Player.Filters {
    /// <summary>
    /// Uses amplification to create a shuddering effect, where the volume quickly oscillates.
    /// </summary>
    public readonly struct TremoloFilter : IFilter {
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