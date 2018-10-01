using Newtonsoft.Json;

namespace Victoria.Objects.Stats
{
    internal sealed class CPU
    {
        [JsonProperty("cores")]
        public int Cores { get; set; }

        [JsonProperty("systemLoad")]
        public double SystemLoad { get; set; }

        [JsonProperty("lavalinkLoad")]
        public double LavalinkLoad { get; set; }
    }
}