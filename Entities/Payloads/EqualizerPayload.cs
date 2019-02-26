using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Victoria.Entities.Payloads
{
    internal sealed class EqualizerPayload : LavaPayload
    {
        [JsonProperty("bands")]
        public List<EqualizerBand> Bands { get; set; }

        public EqualizerPayload(ulong guildId, params EqualizerBand[] bands) : base(guildId, "equalizer")
        {
            if (bands.Any(x => x.Gain > 1.0 || x.Gain < -0.25 || x.Band > 14))
                throw new ArgumentOutOfRangeException(nameof(bands),
                    "Gain value must be between -0.25 - 1.0 and Band value must be between 0 - 14.");

            Bands = new List<EqualizerBand>(bands);
        }

        public EqualizerPayload(ulong guildId, List<EqualizerBand> bands) : base(guildId, "equalizer")
        {
            if (bands.Any(x => x.Gain > 1.0 || x.Gain < -0.25 || x.Band > 14))
                throw new ArgumentOutOfRangeException(nameof(bands),
                    "Gain value must be between -0.25 - 1.0 and Band value must be between 0 - 14.");

            Bands = bands;
        }
    }
}
