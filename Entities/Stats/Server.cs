using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Stats
{
    public struct Server
    {
        [JsonProperty("playingPlayers")]
        public int PlayingPlayers { get; set; }

        [JsonProperty("memory")]
        public Memory Memory { get; set; }

        [JsonProperty("players")]
        public int Players { get; set; }

        [JsonProperty("cpu")]
        public CPU CPU { get; set; }

        [JsonProperty("uptime")]
        private long _Uptime { get; set; }

        [JsonIgnore]
        public TimeSpan Uptime
            => TimeSpan.FromMilliseconds(_Uptime);
    }
}