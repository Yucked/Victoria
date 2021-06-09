using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal sealed record EqualizerPayload : AbstractPlayerPayload {
        [JsonPropertyName("bands"), JsonInclude]
        private IEnumerable<EqualizerBand> Bands { get; }

        public EqualizerPayload(ulong guildId, params EqualizerBand[] equalizerBands) : base(guildId, "equalizer") {
            Bands = equalizerBands;
        }
    }
}