using System.Text.Json.Serialization;

namespace Victoria.Player.Filters {
    /// <summary>
    /// Mixes both channels (left and right), with a configurable factor on how much each channel affects the other.
    /// With the defaults, both channels are kept independent from each other.
    /// Setting all factors to 0.5 means both channels get the same audio.
    /// </summary>
    public struct ChannelMixFilter : IFilter {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("leftToLeft")]
        public double LeftToLeft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("leftToRight")]
        public double LeftToRight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("rightToLeft")]
        public double RightToLeft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("rightToRight")]
        public double RightToRight { get; set; }
    }
}