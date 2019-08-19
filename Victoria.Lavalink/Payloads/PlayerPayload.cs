using Newtonsoft.Json;

namespace Victoria.Lavalink.Payloads
{
    internal abstract class PlayerPayload : BaseLavaPayload
    {
        [JsonProperty("guildId")]
        private string GuildId { get; }

        protected PlayerPayload(ulong guildId, string op) : base(op)
        {
            GuildId = $"{guildId}";
        }
    }
}
