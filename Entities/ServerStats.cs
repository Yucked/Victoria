using System;
using Newtonsoft.Json;

namespace Victoria.Entities
{
    public sealed class ServerStats
    {
        internal ServerStats() { }

        [JsonProperty("playingPlayers")]
        public int PlayingPlayers { get; private set; }

        [JsonProperty("memory")]
        public Memory Memory { get; private set; }

        [JsonProperty("players")]
        public int PlayerCount { get; private set; }

        [JsonProperty("cpu")]
        public Cpu Cpu { get; private set; }

        [JsonProperty("uptime")]
        private long _Uptime { get; set; }

        [JsonIgnore]
        public TimeSpan Uptime
            => TimeSpan.FromMilliseconds(_Uptime);

        [JsonProperty("frameStats")]
        public Frames Frames { get; private set; }
    }
}