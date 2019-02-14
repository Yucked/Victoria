using System.Text.Json.Serialization;

namespace Victoria.Entities.Payloads
{
    internal class LavaPayload : BasePayload
    {
        [JsonPropertyName("guildId")]
        public ulong GuildId { get; }

        protected LavaPayload(ulong guildId, string op) : base(op)
        {
            GuildId = guildId;
        }
    }
}