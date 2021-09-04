using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal sealed class PlayPayload : AbstractPlayerPayload {
        [JsonPropertyName("track")]
        public string Hash { get; }

        [JsonPropertyName("noReplace"),
         JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool NoReplace { get; }

        [JsonPropertyName("startTime"),
         JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int StartTime { get; }

        [JsonPropertyName("endTime"),
         JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int EndTime { get; }

        [JsonPropertyName("volume"),
         JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Volume { get; }

        [JsonPropertyName("pause"),
         JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Pause { get; }

        public PlayPayload(ulong guildId, PlayArgs playArgs)
            : base(guildId, "play") {
            Hash = playArgs.Track.Hash;
            NoReplace = playArgs.NoReplace;
            Volume = playArgs.Volume;
            Pause = playArgs.ShouldPause;
            if (playArgs.StartTime.HasValue) {
                StartTime = (int) playArgs.StartTime.Value.TotalMilliseconds;
            }

            if (playArgs.EndTime.HasValue) {
                EndTime = (int) playArgs.EndTime.Value.TotalMilliseconds;
            }
        }
    }
}