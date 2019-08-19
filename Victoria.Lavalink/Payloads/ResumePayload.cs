using System;
using Newtonsoft.Json;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class ResumePayload : BaseLavaPayload
    {
        [JsonProperty("key")]
        private string Key { get; }

        [JsonProperty("timeout")]
        private long Timeout { get; }

        public ResumePayload(string key, TimeSpan timeout) : base("configureResuming")
        {
            Key = key;
            Timeout = (long) timeout.TotalSeconds;
        }
    }
}
