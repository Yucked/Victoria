using System.Text.Json.Serialization;

namespace Victoria.Lavalink.Payloads
{
    internal abstract class PlayerPayload : BaseLavaPayload
    {
        [JsonPropertyName("guildId")]
        private string GuildId { get; }

        protected PlayerPayload(ulong guildId, string op) : base(op)
        {
            GuildId = $"{guildId}";
        }
    }
}