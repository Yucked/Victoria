using Newtonsoft.Json;

namespace Victoria.Objects.Stats
{
    internal sealed class Frame
    {
        [JsonProperty("sent")]
        public int Sent { get; set; }

        [JsonProperty("nulled")]
        public int Nulled { get; set; }

        [JsonProperty("deficit")]
        public int Deficit { get; set; }
    }
}