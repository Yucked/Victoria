using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Victoria.Payloads {
    internal sealed class EqualizerPayload : PlayerPayload {
        [JsonPropertyName("bands")]
        public IEnumerable<EqualizerBand> Bands { get; }

        public EqualizerPayload(ulong guildId, params EqualizerBand[] bands) : base(guildId, "equalizer") {
            if (bands.Any(x => x.Gain > 1.0 || x.Gain < -0.25 || x.Number > 14))
                throw new ArgumentOutOfRangeException(nameof(bands),
                    "Gain value must be between -0.25 - 1.0 and EqualizerBand value must be between 0 - 14.");

            Bands = bands;
        }

        public EqualizerPayload(ulong guildId, IEnumerable<EqualizerBand> bands) : base(guildId, "equalizer") {
            var enumerable = bands as EqualizerBand[] ?? bands.ToArray();

            if (enumerable.Any(x => x.Gain > 1.0 || x.Gain < -0.25 || x.Number > 14))
                throw new ArgumentOutOfRangeException(nameof(bands),
                    "Gain value must be between -0.25 - 1.0 and EqualizerBand value must be between 0 - 14.");

            Bands = enumerable;
        }
    }
}