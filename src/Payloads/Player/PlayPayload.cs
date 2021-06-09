using System.Text.Json.Serialization;
using Victoria.Player.Args;

namespace Victoria.Payloads.Player {
    internal sealed class PlayPayload : AbstractPlayerPayload {
        [JsonPropertyName("track"), JsonInclude]
        private string Hash { get; }

        [JsonPropertyName("noReplace"), JsonInclude]
        private bool NoReplace { get; }

        [JsonPropertyName("startTime"), JsonInclude]
        private int StartTime { get; }

        [JsonPropertyName("endTime"), JsonInclude]
        private int EndTime { get; }

        [JsonPropertyName("volume"), JsonInclude]
        private int Volume { get; }

        [JsonPropertyName("pause"), JsonInclude]
        private bool Pause { get; }

        public PlayPayload(ulong guildId, PlayArgs playArgs) : base(guildId, "play") {
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