using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Victoria.Rest.Search {
    /// <summary>
    /// 
    /// </summary>
    public struct SearchException {
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