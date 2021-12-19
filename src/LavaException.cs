using System.Text.Json.Serialization;

namespace Victoria {
    /// <summary>
    ///     Exception data given by Lavalink
    /// </summary>
    public struct LavaException {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("message"), JsonInclude]
        public string Message { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("severity"), JsonInclude]
        public string Severity { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cause"), JsonInclude]
        public string Cause { get; private set; }
    }
}