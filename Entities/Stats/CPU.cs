using Newtonsoft.Json;

namespace Victoria.Entities.Stats
{
    public struct CPU
    {
        [JsonProperty("cores")] 
        public int Cores { get; set; }

        [JsonProperty("systemLoad")] 
        public double SystemLoad { get; set; }

        [JsonProperty("lavalinkLoad")] 
        public double LavalinkLoad { get; set; }
    }
}