using System.Text.Json.Serialization;

namespace Victoria.Player.Filters {
    /// <summary>
    /// Uses equalization to eliminate part of a band, usually targeting vocals.
    /// </summary>
    public struct KarokeFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("level")]
        public double Level { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("monoLevel")]
        public double MonoLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("filterBand")]
        public double FilterBand { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("filterWidth")]
        public double FilterWidth { get; set; }
    }
}