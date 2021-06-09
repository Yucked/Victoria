using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal record AbstractPlayerPayload : AbstractPayload {
        [JsonPropertyName("guildId"), JsonInclude]
        public string GuildId { get; }

        protected AbstractPlayerPayload(ulong guildId, string op) : base(op) {
            GuildId = $"{guildId}";
        }
    }
}