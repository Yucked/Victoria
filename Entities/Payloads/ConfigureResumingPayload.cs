using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class ConfigureResumingPayload : BasePayload
    {
        [JsonProperty("key")]
        public string Key { get; }

        [JsonProperty("timeout")]
        public int Timeout { get; }

        public ConfigureResumingPayload(string key, int timeout) : base("configureResuming")
        {
            this.Key = key;
            this.Timeout = timeout;
        }
    }
}
