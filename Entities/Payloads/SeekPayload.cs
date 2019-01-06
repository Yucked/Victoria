using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class SeekPayload : PlayerPayload
    {
        [JsonProperty("position")] 
        public long Position { get; }
        
        public SeekPayload(TimeSpan position, ulong guildId) : base("seek", guildId)
        {
            Position = (long) position.TotalMilliseconds;
        }
    }
}