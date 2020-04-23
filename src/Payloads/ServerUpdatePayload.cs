using System.Text.Json.Serialization;

namespace Victoria.Payloads {
	internal sealed class ServerUpdatePayload : BaseLavaPayload {
		[JsonPropertyName("guildId")]
		public string GuildId { get; set; }

		[JsonPropertyName("sessionId")]
		public string SessionId { get; set; }

		[JsonPropertyName("event")]
		public VoiceServerPayload VoiceServerPayload { get; set; }

		public ServerUpdatePayload() : base("voiceUpdate") {
		}
	}

	internal struct VoiceServerPayload {
		[JsonPropertyName("token")]
		public string Token { get; set; }

		[JsonPropertyName("endpoint")]
		public string Endpoint { get; set; }
	}
}