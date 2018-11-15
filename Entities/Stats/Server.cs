using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Stats
{
    public struct Server
    {
        [JsonProperty("playingPlayers")] 
        public int ActivePlayers { get; set; }

        [JsonProperty("players")] 
        public int TotalPlayers { get; set; }

        [JsonIgnore]
        public TimeSpan Uptime
            => TimeSpan.FromMilliseconds(_uptime);

        [JsonProperty("uptime")] 
        private long _uptime { get; set; }

        [JsonProperty("cpu")] 
        public CPU CPU { get; internal set; }

        [JsonProperty("memory")] 
        public Memory Memory { get; internal set; }

        [JsonProperty("frameStats")] 
        public Frame Frames { get; internal set; }
    }
}