using System;
using System.Text.Json.Serialization;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class ResumePayload : BaseLavaPayload
    {
        [JsonPropertyName("key")]
        private string Key { get; }

        [JsonPropertyName("timeout")]
        private long Timeout { get; }

        public ResumePayload(string key, TimeSpan timeout) : base("configureResuming")
        {
            Key = key;
            Timeout = (long) timeout.TotalSeconds;
        }
    }
}