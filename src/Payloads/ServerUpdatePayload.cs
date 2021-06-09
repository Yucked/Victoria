using System.Text.Json.Serialization;

namespace Victoria.Payloads {
    internal record VoiceServerData(string token, string endpoint);

    internal record ServerUpdatePayload : AbstractPayload {
        [JsonPropertyName("guildId")]
        public string GuildId { get; init; }

        [JsonPropertyName("sessionId")]
        public string SessionId { get; init; }

        [JsonPropertyName("event")]
        public VoiceServerData Data { get; init; }

        public ServerUpdatePayload() : base("voiceUpdate") { }
    }
}