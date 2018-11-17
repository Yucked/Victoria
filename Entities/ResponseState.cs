using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Victoria.Entities
{
    public sealed class ResponseState
    {
        [JsonIgnore]
        public DateTimeOffset Time 
            => new DateTimeOffset(_time * TimeSpan.TicksPerMillisecond + 621_355_968_000_000_000, TimeSpan.Zero);
        
        [JsonProperty("time")]
        private long _time { get; set; }
        
        [JsonProperty("guilds")]
        public IEnumerable<Guild> Guilds { get; private set; }
    }

    public sealed class Guild
    {
        [JsonIgnore]
        public ulong GuildId
            => ulong.TryParse(guildId, out var Id) ? Id : 0;
        
        [JsonProperty("guildId")]
        private string guildId { get; set; }
        
        [JsonProperty("player")]
        public Player Player { get; set; }
    }

    public sealed class Player
    {
        [JsonProperty("track")]
        public string Track { get; set; }
        
        [JsonIgnore]
        public TimeSpan Position 
            => TimeSpan.FromMilliseconds(position);
        
        [JsonProperty("position")]
        private int position { get; set; }
        
        [JsonProperty("paused")]
        public bool IsPaused { get; set; }
    }
}