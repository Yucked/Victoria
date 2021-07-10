using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal sealed class PausePayload : AbstractPlayerPayload {
        [JsonPropertyName("pause")]
        public bool Pause { get; }

        public PausePayload(ulong guildId, bool pause) : base(guildId, "pause") {
            Pause = pause;
        }
    }
}