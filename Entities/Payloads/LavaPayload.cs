using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal abstract class LavaPayload
    {
        [JsonProperty("op")] 
        public string Operation { get; }

        [JsonProperty("guildId")] 
        public string GuildId { get; }
        
        protected LavaPayload(string op, ulong guildId)
        {
            Operation = op;
            GuildId = $"{guildId}";
        }
    }
}