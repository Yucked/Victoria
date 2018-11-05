using System.Collections.Generic;
using Newtonsoft.Json;
using Victoria.Objects;

namespace Victoria.Payloads
{
    internal sealed class EqualizerPayload : LavaPayload
    {
        public EqualizerPayload(ulong id, params EqualizerBand[] bands) : base("equalizer", id)
        {
            Bands = new HashSet<EqualizerBand>(bands);
        }

        [JsonProperty("bands")] 
        public HashSet<EqualizerBand> Bands { get; set; }
    }
}