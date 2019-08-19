namespace Victoria.Frostbyte.Infos.Metrics
{
    /// <summary>
    ///     Basic process memory usage.
    /// </summary>
    public struct MemoryInfo
    {
        /// <summary>
        ///     Returned via GC.Collect(force: true)
        /// </summary>
        public long Used { get; set; }

        /// <summary>
        ///     Returned via Process.VirtualMemorySize64.
        /// </summary>
        public long Allocated { get; set; }
    }
}