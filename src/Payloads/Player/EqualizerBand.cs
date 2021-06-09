using System;
using System.Linq;

namespace Victoria.Payloads.Player {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct EqualizerBand {
        /// <summary>
        /// 
        /// </summary>
        public int Band { get; }

        /// <summary>
        /// 
        /// </summary>
        public double Gain { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="band"></param>
        /// <param name="gain"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public EqualizerBand(int band, double gain) {
            if (!Enumerable.Range(0, 15).Contains(band)) {
                throw new ArgumentOutOfRangeException(nameof(band), "Valid bands are from 0 - 14.");
            }

            if (gain < -0.25 || gain > 1.0) {
                throw new ArgumentOutOfRangeException(nameof(gain), "Valid gains are from -0.25 - 1.0.");
            }

            Band = band;
            Gain = gain;
        }
    }
}