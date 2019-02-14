using System.Text.Json.Serialization;

namespace Victoria.Entities.Payloads
{
    internal abstract class BasePayload
    {
        [JsonPropertyName("op")]
        public string Op { get; }

        protected BasePayload(string op)
        {
            Op = op;
        }
    }
}