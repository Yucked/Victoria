using Newtonsoft.Json;

namespace Victoria.Lavalink.Payloads
{
    internal abstract class BaseLavaPayload
    {
        [JsonProperty("op")]
        public string Op { get; }

        protected BaseLavaPayload(string op)
        {
            Op = op;
        }
    }
}
