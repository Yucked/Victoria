using System.Text.Json.Serialization;

namespace Victoria.Player.Filters {
    /// <summary>
    /// Similar to tremolo. While tremolo oscillates the volume, vibrato oscillates the pitch.
    /// </summary>
    public struct VibratoFilter : IFilter {
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