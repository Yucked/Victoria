using System.Text.Json.Serialization;

namespace Victoria.Responses.Route {
    /// <summary>
    ///     Contains information about route planner class and details.
    /// </summary>
    public record RouteStatus {
        /// <summary>
        ///     Which planner class is being used.
        /// </summary>
        [JsonPropertyName("class"), JsonInclude]
        public string Class { get; internal set; }

        /// <summary>
        ///     Gives more information about route planner.
        /// </summary>
        [JsonPropertyName("details"), JsonInclude]
        public RouteDetail Details { get; internal set; }
    }
}