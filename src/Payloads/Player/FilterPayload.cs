using System.Collections.Generic;
using System.Text.Json.Serialization;
using Victoria.Player.Filters;

namespace Victoria.Payloads.Player {
    internal sealed class FilterPayload : AbstractPlayerPayload {
        [JsonPropertyName("volume")]
        public double Volume { get; }

        [JsonPropertyName("equalizer")]
        public IEnumerable<EqualizerBand> Bands { get; }

        [JsonPropertyName("karoke")]
        public KarokeFilter Karoke { get; set; }

        [JsonPropertyName("timescale")]
        public TimescaleFilter Timescale { get; set; }

        [JsonPropertyName("tremolo")]
        public TremoloFilter Tremolo { get; set; }

        [JsonPropertyName("vibrato")]
        public VibratoFilter Vibrato { get; set; }

        [JsonPropertyName("rotation")]
        public RotationFilter Rotation { get; set; }

        [JsonPropertyName("distortion")]
        public DistortionFilter Distortion { get; set; }

        [JsonPropertyName("channelMix")]
        public ChannelMixFilter ChannelMix { get; set; }

        [JsonPropertyName("lowPass")]
        public LowPassFilter LowPass { get; set; }

        public FilterPayload(ulong guildId, IFilter filter, double volume, IEnumerable<EqualizerBand> bands)
            : base(guildId, "filters") {
            Volume = volume;
            Bands = bands;
            SetFilter(filter);
        }

        public FilterPayload(ulong guildId,
                             IEnumerable<IFilter> filters,
                             double volume,
                             IEnumerable<EqualizerBand> bands)
            : base(guildId, "filters") {
            Volume = volume;
            Bands = bands;
            foreach (var filter in filters) {
                SetFilter(filter);
            }
        }

        private void SetFilter(IFilter filter) {
            switch (filter) {
                case KarokeFilter karokeFilter:
                    Karoke = karokeFilter;
                    break;

                case TimescaleFilter timescaleFilter:
                    Timescale = timescaleFilter;
                    break;

                case TremoloFilter tremoloFilter:
                    Tremolo = tremoloFilter;
                    break;

                case VibratoFilter vibratoFilter:
                    Vibrato = vibratoFilter;
                    break;

                case RotationFilter rotationFilter:
                    Rotation = rotationFilter;
                    break;

                case DistortionFilter distortionFilter:
                    Distortion = distortionFilter;
                    break;

                case ChannelMixFilter channelMixFilter:
                    ChannelMix = channelMixFilter;
                    break;

                case LowPassFilter lowPassFilter:
                    LowPass = lowPassFilter;
                    break;
            }
        }
    }
}