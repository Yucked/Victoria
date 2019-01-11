using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class PlayPayload : PlayerPayload
    {
        [JsonProperty("track")] 
        public string Track { get; }

        [JsonProperty("startTime")] 
        public long StartTime { get; }

        [JsonProperty("stopTime")] 
        public long StopTime { get; }
        
        [JsonProperty("noReplace")]
        public bool ShouldReplace { get; }

        public PlayPayload(string trackString, TimeSpan start, TimeSpan stop, bool shouldReplace, ulong guildId) : base("play", guildId)
        {
            Track = trackString;
            StartTime = (long) start.TotalMilliseconds;
            StopTime = (long) stop.TotalMilliseconds;
            ShouldReplace = shouldReplace;
        }
    }
}