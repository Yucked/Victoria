using System.Text.Json.Serialization;

namespace Victoria.Player.Filters {
    /// <summary>
    /// Uses amplification to create a shuddering effect, where the volume quickly oscillates.
    /// </summary>
    public struct TremoloFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("frequency")]
        public double Frequency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("depth")]
        public double Depth { get; set; }
    }
}