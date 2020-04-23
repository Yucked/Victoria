using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Victoria.Payloads {
	internal sealed class EqualizerPayload : PlayerPayload {
		[JsonPropertyName("bands")]
		public IEnumerable<EqualizerBand> Bands { get; }

		public EqualizerPayload(ulong guildId, params EqualizerBand[] bands) : base(guildId, "equalizer") {
			Bands = bands;
		}
	}
}