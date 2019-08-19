using System;
using Newtonsoft.Json;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class SeekPayload : PlayerPayload
    {
        [JsonProperty("position")]
        private long Position { get; }

        public SeekPayload(ulong guildId, TimeSpan position) : base(guildId, "seek")
        {
            Position = (long) position.TotalMilliseconds;
        }
    }
}
