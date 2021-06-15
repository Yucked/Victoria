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
        public KarokeFilter Karoke { get; }

        [JsonPropertyName("timescale")]
        public TimescaleFilter Timescale { get; }

        [JsonPropertyName("tremolo")]
        public TremoloFilter Tremolo { get; }

        [JsonPropertyName("vibrato")]
        public VibratoFilter Vibrato { get; }
        
        [JsonPropertyName("rotation")]
        public RotationFilter Rotation { get; }
        
        [JsonPropertyName("distortion")]
        public DistortionFilter Distortion { get; }
        
        [JsonPropertyName("channelMix")]
        public ChannelMixFilter ChannelMix { get; }
        
        [JsonPropertyName("lowPass")]
        public LowPassFilter LowPass { get; }

        public FilterPayload(ulong guildId, IFilter filter, double volume, IEnumerable<EqualizerBand> bands)
            : base(guildId, "filters") {
            Volume = volume;
            Bands = bands;

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