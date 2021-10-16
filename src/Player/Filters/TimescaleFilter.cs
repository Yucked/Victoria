using System.Text.Json.Serialization;

namespace Victoria.Player.Filters {
    /// <summary>
    /// Changes the speed, pitch, and rate. All default to 1.
    /// </summary>
    public struct TimescaleFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("speed")]
        public double Speed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("pitch")]
        public double Pitch { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("rate")]
        public double Rate { get; set; }
    }
}