using Newtonsoft.Json;

namespace Victoria.Entities
{
    public struct EqualizerBand
    {
        [JsonProperty("band")]
        public ushort Band { get; set; }
        
        [JsonProperty("gain")]
        public double Gain { get; set; }
    }
}