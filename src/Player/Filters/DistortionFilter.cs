using System.Text.Json.Serialization;

namespace Victoria.Player.Filters {
    /// <summary>
    /// Distortion effect. It can generate some pretty unique audio effects.
    /// </summary>
    public struct DistortionFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("sinOffset")]
        public int SinOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("sinScale")]
        public int SinScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cosOffset")]
        public int CosOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cosScale")]
        public int CosScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("tanOffset")]
        public int TanOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("tanScale")]
        public int TanScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("scale")]
        public int Scale { get; set; }
    }
}