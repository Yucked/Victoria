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
        public double SinOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("sinScale")]
        public double SinScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cosOffset")]
        public double CosOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cosScale")]
        public double CosScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("tanOffset")]
        public double TanOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("tanScale")]
        public double TanScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("offset")]
        public double Offset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("scale")]
        public double Scale { get; set; }
    }
}