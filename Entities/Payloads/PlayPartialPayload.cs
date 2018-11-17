using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class PlayPartialPayload : LavaPayload
    {
        [JsonProperty("track")] 
        public string Track { get; }

        [JsonProperty("startTime")] 
        public long StartTime { get; }

        [JsonProperty("stopTime")] 
        public long StopTime { get; }

        public PlayPartialPayload(string trackString, TimeSpan start, TimeSpan stop, ulong guildId) : base("play", guildId)
        {
            Track = trackString;
            StartTime = (long) start.TotalMilliseconds;
            StopTime = (long) stop.TotalMilliseconds;
        }
    }
}