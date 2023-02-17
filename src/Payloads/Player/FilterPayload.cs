using System.Collections.Generic;
using System.Text.Json.Serialization;
using Victoria.Player.Filters;

namespace Victoria.Payloads.Player {
    internal sealed class FilterPayload : AbstractPlayerPayload {
        [JsonPropertyName("volume"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double Volume { get; }

        [JsonPropertyName("equalizer"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public IEnumerable<EqualizerBand> Bands { get; }

        [JsonPropertyName("karoke"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public KarokeFilter Karoke { get; set; }

        [JsonPropertyName("timescale"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public TimescaleFilter Timescale { get; set; }

        [JsonPropertyName("tremolo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public TremoloFilter Tremolo { get; set; }

        [JsonPropertyName("vibrato"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public VibratoFilter Vibrato { get; set; }

        [JsonPropertyName("rotation"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public RotationFilter Rotation { get; set; }

        [JsonPropertyName("distortion"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DistortionFilter Distortion { get; set; }

        [JsonPropertyName("channelMix"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ChannelMixFilter ChannelMix { get; set; }

        [JsonPropertyName("lowPass"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
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