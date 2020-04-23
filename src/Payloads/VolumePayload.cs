using System.Text.Json.Serialization;

namespace Victoria.Payloads {
	internal sealed class VolumePayload : PlayerPayload {
		[JsonPropertyName("volume")]
		public int Volume { get; }

		public VolumePayload(ulong guildId, int volume) : base(guildId, "volume") {
			Volume = volume;
		}
	}
}