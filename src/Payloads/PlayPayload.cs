using System;
using System.Text.Json.Serialization;

namespace Victoria.Payloads {
    internal sealed class PlayPayload : PlayerPayload {
        [JsonPropertyName("endTime")]
        public int EndTime { get; }

        [JsonPropertyName("track")]
        public string Hash { get; }

        [JsonPropertyName("noReplace")]
        public bool NoReplace { get; }

        [JsonPropertyName("startTime")]
        public int StartTime { get; }

        public PlayPayload(ulong guildId, LavaTrack track, bool noReplace) : base(guildId, "play") {
            Hash = track.Hash;
            NoReplace = noReplace;
        }

        public PlayPayload(ulong guildId, string trackHash,
                           TimeSpan start, TimeSpan end,
                           bool noReplace) : base(guildId, "play") {
            Hash = trackHash;
            StartTime = (int)start.TotalMilliseconds;
            EndTime = (int)end.TotalMilliseconds;
            NoReplace = noReplace;
        }
    }
}