using System;
using System.Text.Json.Serialization;

namespace Victoria.Payloads {
	internal sealed class SeekPayload : PlayerPayload {
		[JsonPropertyName("position")]
		public long Position { get; }

		public SeekPayload(ulong guildId, TimeSpan position) : base(guildId, "seek") {
			Position = (long) position.TotalMilliseconds;
		}
	}
}