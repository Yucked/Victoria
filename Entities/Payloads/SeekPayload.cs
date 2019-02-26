using Newtonsoft.Json;
using System;

namespace Victoria.Entities.Payloads
{
    internal sealed class SeekPayload : LavaPayload
    {
        [JsonProperty("position")]
        public long Position { get; }

        public SeekPayload(ulong guildId, TimeSpan position) : base(guildId, "seek")
        {
            Position = (long)position.TotalMilliseconds;
        }
    }
}