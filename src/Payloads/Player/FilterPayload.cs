using System.Collections.Generic;
using System.Text.Json.Serialization;
using Victoria.Player;
using Victoria.Player.Filters;

namespace Victoria.Payloads.Player {
    internal sealed class FilterPayload : AbstractPlayerPayload {
        [JsonPropertyName("volume")]
        public int Volume { get; }

        [JsonPropertyName("equalizer")]
        public IEnumerable<EqualizerBand> Bands { get; }
        
        public FilterPayload(ulong guildId)
            : base(guildId, "filters") { }
    }
}