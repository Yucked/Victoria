using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Victoria.Responses {
    /// <summary>
    ///     If LoadStatus was LoadFailed then Exception is returned.
    /// </summary>
    public struct LavaException {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("message"), JsonInclude]
        public string Message { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("severity"), JsonInclude]
        public string Severity { get; internal init; }
    }
}