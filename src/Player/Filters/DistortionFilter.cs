namespace Victoria.Player.Filters {
    /// <summary>
    /// Distortion effect. It can generate some pretty unique audio effects.
    /// </summary>
    public struct DistortionFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public int SinOffset { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public int SinScale { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public int CosOffset { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public int CosScale { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public int TanOffset { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public int TanScale { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public int Offset { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public int Scale { get; init; }
    }
}