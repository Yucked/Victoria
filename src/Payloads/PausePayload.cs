using System.Text.Json.Serialization;

namespace Victoria.Payloads {
	internal sealed class PausePayload : PlayerPayload {
		[JsonPropertyName("pause")]
		public bool Pause { get; }

		public PausePayload(ulong guildId, bool pause) : base(guildId, "pause") {
			Pause = pause;
		}
	}
}