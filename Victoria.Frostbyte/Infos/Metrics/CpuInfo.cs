namespace Victoria.Frostbyte.Infos.Metrics
{
    /// <summary>
    ///     Basic CPU Info.
    /// </summary>
    public struct CpuInfo
    {
        /// <summary>
        ///     CPU Cores.
        /// </summary>
        public int Cores { get; private set; }

        /// <summary>
        ///     Underwork.
        /// </summary>
        public double SystemLoad { get; private set; }

        /// <summary>
        ///     Underwork.
        /// </summary>
        public double ProcessLoad { get; private set; }
    }
}
