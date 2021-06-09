using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal sealed record VolumePayload : AbstractPlayerPayload {
        [JsonPropertyName("volume"), JsonInclude]
        private int Volume { get; }

        public VolumePayload(ulong guildId, int volume) : base(guildId, "volume") {
            Volume = volume;
        }
    }
}