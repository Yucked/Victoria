using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal sealed class VolumePayload : AbstractPlayerPayload {
        [JsonPropertyName("volume")]
        public int Volume { get; }

        public VolumePayload(ulong guildId, int volume) : base(guildId, "volume") {
            Volume = volume;
        }
    }
}