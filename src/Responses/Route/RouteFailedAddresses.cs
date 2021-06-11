using System.Text.Json.Serialization;

namespace Victoria.Responses.Route {
    /// <summary>
    /// 
    /// </summary>
    public sealed class RouteFailedAddresses {
        /// <summary>
        ///     Address that is failing.
        /// </summary>
        /// <summary>
        ///     IP Address
        /// </summary>
        [JsonPropertyName("address"), JsonInclude]
        public string Address { get; internal set; }

        /// <summary>
        ///     UNIX Epoch representation of timestamp
        /// </summary>
        [JsonPropertyName("timestamp"), JsonInclude]
        public long Timestamp { get; internal set; }

        /// <summary>
        ///     Time when this address failed.
        /// </summary>
        [JsonPropertyName("failedOn"), JsonInclude]
        public string FailedOn { get; internal set; }
    }
}