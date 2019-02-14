using Discord.WebSocket;
using System.Text.Json.Serialization;

namespace Victoria.Entities.Payloads
{
    internal sealed class VoiceServerPayload : LavaPayload
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; }

        [JsonPropertyName("event")]
        public VoiceServerUpdate VoiceServerUpdate { get; }

        public VoiceServerPayload(SocketVoiceServer server, string voiceSessionId)
            : base(server.Guild.Id, "voiceUpdate")
        {
            SessionId = voiceSessionId;
            VoiceServerUpdate = new VoiceServerUpdate(server);
        }
    }
}