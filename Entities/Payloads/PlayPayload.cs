using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class PlayPayload : LavaPayload
    {
        [JsonProperty("track")]
        public string Hash { get; }

        [JsonProperty("startTime")]
        public int StartTime { get; }

        [JsonProperty("endTime")]
        public int EndTime { get; }

        [JsonProperty("noReplace")]
        public bool NoReplace { get; }

        public PlayPayload(ulong guildId, string trackHash,
                              TimeSpan start, TimeSpan end,
                              bool noReplace) : base(guildId, "play")
        {
            Hash = trackHash;
            StartTime = (int)start.TotalMilliseconds;
            EndTime = (int)end.TotalMilliseconds;
            NoReplace = noReplace;
        }

        public PlayPayload(ulong guildId, string trackHash, bool noReplace) : base(guildId, "play")
        {
            Hash = trackHash;
            NoReplace = noReplace;
        }
    }
}