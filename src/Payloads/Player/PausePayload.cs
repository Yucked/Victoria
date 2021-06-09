using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal record PausePayload : AbstractPlayerPayload {
        [JsonPropertyName("pause"), JsonInclude]
        private bool IsPaused { get; }

        public PausePayload(ulong guildId, bool isPaused) : base(guildId, "pause") {
            IsPaused = isPaused;
        }
    }
}