using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal abstract class BasePayload
    {
        [JsonProperty("op")]
        public string Op { get; }

        protected BasePayload(string op)
        {
            Op = op;
        }
    }
}