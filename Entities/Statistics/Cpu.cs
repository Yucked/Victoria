using Newtonsoft.Json;

namespace Victoria.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Cpu
    {
        internal Cpu() { }

        [JsonProperty("cores")]
        public int Cores { get; private set; }

        [JsonProperty("systemLoad")]
        public double SystemLoad { get; private set; }

        [JsonProperty("lavalinkLoad")]
        public double LavalinkLoad { get; private set; }
    }
}