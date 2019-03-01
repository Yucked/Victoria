using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal class LavaPayload : BasePayload
    {
        [JsonProperty("guildId")]
        public string GuildId { get; }

        protected LavaPayload(ulong guildId, string op) : base(op)
        {
            GuildId = $"{guildId}";
        }
    }
}