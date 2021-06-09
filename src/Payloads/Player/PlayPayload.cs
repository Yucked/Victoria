using System;
using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal sealed record PlayPayload : AbstractPlayerPayload {
        [JsonPropertyName("endTime"), JsonInclude]
        private int EndTime { get; }

        [JsonPropertyName("track"), JsonInclude]
        private string Hash { get; }

        [JsonPropertyName("noReplace"), JsonInclude]
        private bool NoReplace { get; }

        [JsonPropertyName("startTime"), JsonInclude]
        private int StartTime { get; }

        [JsonPropertyName("volume"), JsonInclude]
        private int Volume { get; }

        [JsonPropertyName("pause"), JsonInclude]
        private bool Pause { get; }

        public PlayPayload(ulong guildId, string hash, bool noReplace, int volume, bool shouldPause)
            : base(guildId, "play") {
            Hash = hash;
            NoReplace = noReplace;
            Volume = volume;
            Pause = shouldPause;
        }

        public PlayPayload(ulong guildId, string hash,
                           TimeSpan startTime, TimeSpan endTime,
                           bool noReplace, int volume,
                           bool shouldPause)
            : base(guildId, "play") {
            Hash = hash;
            StartTime = (int) startTime.TotalMilliseconds;
            EndTime = (int) endTime.TotalMilliseconds;
            NoReplace = noReplace;
            Volume = volume;
            Pause = shouldPause;
        }
    }
}