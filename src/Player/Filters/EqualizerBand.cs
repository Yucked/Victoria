using System;
using System.Linq;
using System.Text.Json.Serialization;

#pragma warning disable 8632

namespace Victoria.Player.Filters {
    /// <summary>
    ///     Equalizer band
    /// </summary>
    public readonly struct EqualizerBand : IEquatable<EqualizerBand> {
        /// <summary>
        ///     15 bands (0-14) that can be changed.
        /// </summary>
        [JsonPropertyName("band")]
        public int Band { get; }

        /// <summary>
        ///     Gain is the multiplier for the given band. The default value is 0. Valid values range from -0.25 to 1.0,
        ///     where -0.25 means the given band is completely muted, and 0.25 means it is doubled.
        /// </summary>
        [JsonPropertyName("gain")]
        public double Gain { get; }

        /// <summary>
        /// </summary>
        /// <param name="band"></param>
        /// <param name="gain"></param>
        public EqualizerBand(int band, double gain) {
            Gain = gain is < -0.25 or > 1.0
                ? throw new ArgumentOutOfRangeException(nameof(gain), "Valid gains are from -0.25 - 1.0.")
                : gain;

            Band = !Enumerable.Range(0, 15).Contains(band)
                ? throw new ArgumentOutOfRangeException(nameof(band), "Valid bands are from 0 - 14.")
                : band;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) {
            if (obj is not EqualizerBand equalizerBand) {
                return false;
            }

            return equalizerBand.Band == Band;
        }

        /// <inheritdoc />
        public bool Equals(EqualizerBand other) {
            return Band == other.Band;
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                return (Band.GetHashCode() * 397) ^ Gain.GetHashCode();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(EqualizerBand left, EqualizerBand right) {
            return left.Equals(right);
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(EqualizerBand left, EqualizerBand right) {
            return !left.Equals(right);
        }
    }
}