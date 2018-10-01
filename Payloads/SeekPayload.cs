using System;
using Newtonsoft.Json;

namespace Victoria.Payloads
{
    internal sealed class SeekPayload : LavaPayload
    {
        [JsonProperty("position")]
        public long Position { get; }
        
        public SeekPayload(TimeSpan position, ulong id) : base("seek", id)
        {
            Position = (long) position.TotalMilliseconds;
        }
    }
}