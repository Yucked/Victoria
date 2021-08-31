using System.Text.Json.Serialization;

namespace Victoria.Responses.Route {
    /// <summary>
    ///     Represents the current IP address being used.
    /// </summary>
    public sealed class RouteIPBlock {
        /// <summary>
        ///     Type of IP address being used.
        /// </summary>
        [JsonPropertyName("type"), JsonInclude]
        public string Type { get; private set; }

        /// <summary>
        ///     Size of IP address?
        /// </summary>
        [JsonPropertyName("size"), JsonInclude]
        public string Size { get; internal set; }
    }
}