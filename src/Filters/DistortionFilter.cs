namespace Victoria.Filters {
    /// <summary>
    /// Distortion effect. It can generate some pretty unique audio effects.
    /// </summary>
    public struct DistortionFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        public int SinOffset { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int SinScale { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int CosOffset { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int CosScale { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int TanOffset { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int TanScale { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int Offset { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int Scale { get; set; }
    }
}