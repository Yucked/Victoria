using Newtonsoft.Json;

namespace Victoria.Entities.Statistics
{
    public struct CPU
    {
        [JsonProperty("cores")]
        public int Cores { get; private set; }

        [JsonProperty("systemLoad")]
        public double SystemLoad { get; private set; }

        [JsonProperty("lavalinkLoad")]
        public double LavalinkLoad { get; private set; }
    }
}