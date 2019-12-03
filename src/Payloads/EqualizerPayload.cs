using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Victoria.Payloads {
    internal sealed class EqualizerPayload : PlayerPayload {
        [JsonPropertyName("bands")]
        public IEnumerable<Band> Bands { get; }

        public EqualizerPayload(ulong guildId, params Band[] bands) : base(guildId, "equalizer") {
            if (bands.Any(x => x.Gain > 1.0 || x.Gain < -0.25 || x.Number > 14))
                throw new ArgumentOutOfRangeException(nameof(bands),
                    "Gain value must be between -0.25 - 1.0 and Band value must be between 0 - 14.");

            Bands = bands;
        }

        public EqualizerPayload(ulong guildId, IEnumerable<Band> bands) : base(guildId, "equalizer") {
            var enumerable = bands as Band[] ?? bands.ToArray();

            if (enumerable.Any(x => x.Gain > 1.0 || x.Gain < -0.25 || x.Number > 14))
                throw new ArgumentOutOfRangeException(nameof(bands),
                    "Gain value must be between -0.25 - 1.0 and Band value must be between 0 - 14.");

            Bands = enumerable;
        }
    }
}