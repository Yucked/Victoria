using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Statistics
{
    public struct Stats
    {
        [JsonProperty("playingPlayers")]
        public int PlayingPlayers { get; private set; }

        [JsonProperty("memory")]
        public Memory Memory { get; private set; }

        [JsonProperty("players")]
        public int Players { get; private set; }

        [JsonProperty("cpu")]
        public CPU CPU { get; private set; }

        [JsonProperty("uptime")]
        private long _Uptime { get; set; }

        [JsonIgnore]
        public TimeSpan Uptime
            => TimeSpan.FromMilliseconds(_Uptime);   
        
        [JsonProperty("frameStats", NullValueHandling = NullValueHandling.Ignore)]
        public Frames Frames { get; private set; }
    }
}