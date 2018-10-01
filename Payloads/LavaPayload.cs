using Newtonsoft.Json;

namespace Victoria.Payloads
{
    internal abstract class LavaPayload
    {        
        [JsonProperty("op")] 
        public string Operation { get; }

        [JsonProperty("guildId")] 
        public string GuildId { get; }

        protected LavaPayload(string op, ulong id)
        {
            Operation = op;
            GuildId = $"{id}";
        }
    }
}