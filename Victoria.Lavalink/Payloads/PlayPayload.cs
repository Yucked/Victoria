using System;
using Newtonsoft.Json;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class PlayPayload : PlayerPayload
    {
        [JsonProperty("track")]
        private string Hash { get; }

        [JsonProperty("startTime")]
        private int StartTime { get; }

        [JsonProperty("endTime")]
        private int EndTime { get; }

        [JsonProperty("noReplace")]
        private bool NoReplace { get; }

        public PlayPayload(ulong guildId, LavaTrack track, bool noReplace) : base(guildId, "play")
        {
            Hash = track.Hash;
            StartTime = 0;
            EndTime = (int) track.Duration.TotalMilliseconds;
            NoReplace = noReplace;
        }

        public PlayPayload(ulong guildId, string trackHash,
            TimeSpan start, TimeSpan end,
            bool noReplace) : base(guildId, "play")
        {
            Hash = trackHash;
            StartTime = (int) start.TotalMilliseconds;
            EndTime = (int) end.TotalMilliseconds;
            NoReplace = noReplace;
        }
    }
}
