using System.Text.Json.Serialization;
using Victoria.Player;
using Victoria.Player.Args;

namespace Victoria.Payloads.Player {
    internal sealed class PlayPayload<TLavaTrack> : AbstractPlayerPayload
        where TLavaTrack : LavaTrack {
        [JsonPropertyName("track")]
        public string Hash { get; }

        [JsonPropertyName("noReplace")]
        public bool NoReplace { get; }

        [JsonPropertyName("startTime")]
        public int StartTime { get; }

        [JsonPropertyName("endTime")]
        public int EndTime { get; }

        [JsonPropertyName("volume")]
        public int Volume { get; }

        [JsonPropertyName("pause")]
        public bool Pause { get; }

        public PlayPayload(ulong guildId, PlayArgs<TLavaTrack> playArgs) : base(guildId, "play") {
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