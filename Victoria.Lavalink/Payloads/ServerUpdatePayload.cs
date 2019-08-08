using System.Text.Json.Serialization;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class ServerUpdatePayload : BaseLavaPayload
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        [JsonPropertyName("event")]
        public VoiceServerPayload VoiceServerPayload { get; set; }

        public ServerUpdatePayload() : base("voiceUpdate")
        {
        }
    }

    internal struct VoiceServerPayload
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("guildId")]
        public string GuildId { get; set; }

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; }
    }
}