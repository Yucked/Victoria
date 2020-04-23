using System.Text.Json.Serialization;

namespace Victoria.Payloads {
	internal abstract class PlayerPayload : BaseLavaPayload {
		[JsonPropertyName("guildId")]
		public string GuildId { get; }

		protected PlayerPayload(ulong guildId, string op) : base(op) {
			GuildId = $"{guildId}";
		}
	}
}