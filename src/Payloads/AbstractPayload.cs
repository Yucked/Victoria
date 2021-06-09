using System.Text.Json.Serialization;

namespace Victoria.Payloads {
    internal abstract class AbstractPayload {
        [JsonPropertyName("op")]
        public string Op { get; }

        protected AbstractPayload(string op) {
            Op = op;
        }
    }
}