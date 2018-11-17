using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal abstract partial class LavaPayload
    {
        [JsonProperty("guildId")] 
        public string GuildId { get; }
        
        protected LavaPayload(string op, ulong guildId)
        {
            Operation = op;
            GuildId = $"{guildId}";
        }
    }

    internal abstract partial class LavaPayload
    {
        [JsonProperty("op")]
        public string Operation { get; set; }

        protected LavaPayload(string op)
        {
            Operation = op;
        }
    }
}