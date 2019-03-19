using Newtonsoft.Json;
using System;

namespace Victoria.Entities
{
    internal struct PlayerState
    {
        [JsonIgnore]
        public DateTimeOffset Time
            => DateTimeOffset.FromUnixTimeMilliseconds(LongTime);

        [JsonProperty("time")]
        private long LongTime { get; set; }

        [JsonIgnore]
        public TimeSpan Position
            => TimeSpan.FromMilliseconds(LongPosition);

        [JsonProperty("position")]
        private long LongPosition { get; set; }
    }
}