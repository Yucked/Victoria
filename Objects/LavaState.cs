using System;
using Newtonsoft.Json;

namespace Victoria.Objects
{
    internal sealed class LavaState
    {
        [JsonIgnore]
        public DateTimeOffset Time
            => new DateTimeOffset(_time * TimeSpan.TicksPerMillisecond + 621_355_968_000_000_000, TimeSpan.Zero);
        
        [JsonProperty("time")]
        private long _time { get; set; }

        [JsonIgnore]
        public TimeSpan Position 
            => TimeSpan.FromMilliseconds(_position);
        
        [JsonProperty("position")]
        private long _position { get; set; }
    }
}