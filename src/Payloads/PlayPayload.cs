using System;
using System.Text.Json.Serialization;

namespace Victoria.Payloads
{
    internal sealed class PlayPayload : PlayerPayload
    {
        [JsonPropertyName("track")]
        public string Hash { get; }

        [JsonPropertyName("startTime")]
        public int StartTime { get; }

        [JsonPropertyName("endTime")]
        public int EndTime { get; }

        [JsonPropertyName("noReplace")]
        public bool NoReplace { get; }

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