using Newtonsoft.Json;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class ServerUpdatePayload : BaseLavaPayload
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        [JsonProperty("event")]
        public VoiceServerPayload VoiceServerPayload { get; set; }

        public ServerUpdatePayload() : base("voiceUpdate")
        {
        }
    }

    internal struct VoiceServerPayload
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("guildId")]
        public string GuildId { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }
}
