using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal abstract class PlayerPayload : LavaPayload
    {
        [JsonProperty("guildId")] 
        public string GuildId { get; }
        
        protected PlayerPayload(string op, ulong guildId) : base(op)
        {
            GuildId = $"{guildId}";
        }
    }
}