using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class ResumePayload : LavaPayload
    {
        [JsonProperty("key")]
        public string Key { get; }

        [JsonProperty("timeout")]
        public int Timeout { get; }
        
        public ResumePayload(string key, TimeSpan? timeSpan) : base("configureResuming")
        {
            Key = key;
            Timeout = timeSpan?.Seconds ?? TimeSpan.FromSeconds(60).Seconds;
        }        
    }
}