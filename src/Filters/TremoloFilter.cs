namespace Victoria.Filters {
    /// <summary>
    /// Uses amplification to create a shuddering effect, where the volume quickly oscillates.
    /// </summary>
    public struct TremoloFilter : IFilter {
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