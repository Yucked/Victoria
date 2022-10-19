using System.Text.Json.Serialization;

namespace Victoria.Payloads {
    internal record VoiceServerPayload {
        [JsonPropertyName("token")]
        public string Token { get; init; }

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; init; }
    }
}