using System.Text.Json.Serialization;

namespace Victoria.Payloads {
    internal record AbstractPayload {
        [JsonPropertyName("op"), JsonInclude]
        public string Op { get; }

        protected AbstractPayload(string op) {
            Op = op;
        }
    }
}