using System.Text.Json.Serialization;

namespace Victoria.Responses.Route {
    internal record RouteResponse {
        [JsonPropertyName("error"), JsonInclude]
        public string Error { get; private set; }

        [JsonPropertyName("message"), JsonInclude]
        public string Message { get; private set; }
    }
}