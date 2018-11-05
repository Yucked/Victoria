using System;
using Newtonsoft.Json;

namespace Victoria.Objects
{
    public sealed class EqualizerBand
    {
        internal EqualizerBand()
        {
        }

        /// <param name="band">15 bands (0-14) that can be changed.</param>
        /// <param name="gain">Gain is the multiplier for the given band. The default value is 0.
        /// Valid values range from -0.25 to 1.0, where -0.25 means the given band is completely muted, and 0.25 means it is doubled</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if any of the values are out of range.</exception>
        public EqualizerBand(int band, double gain = 0)
        {
            if (band < 0)
                throw new ArgumentOutOfRangeException(nameof(band), "Cannot be less than 0.");

            if (band > 14)
                throw new ArgumentOutOfRangeException(nameof(band), "Cannot be greater than 14.");

            if (gain < -0.25)
                throw new ArgumentOutOfRangeException(nameof(gain), "Cannot be less than -0.25.");

            if (gain > 0.25)
                throw new ArgumentOutOfRangeException(nameof(gain), "Cannot be greater than -0.25.");
            
            Band = band;
            Gain = gain;
        }

        [JsonProperty("band")] 
        public int Band { get; set; }

        [JsonProperty("gain")] 
        public double Gain { get; set; }
    }
}