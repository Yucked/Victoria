using Discord.WebSocket;
using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class VoiceServerPayload : LavaPayload
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; }

        [JsonProperty("event")]
        public VoiceServerUpdate VoiceServerUpdate { get; }

        public VoiceServerPayload(SocketVoiceServer server, string voiceSessionId)
            : base(server.Guild.Id, "voiceUpdate")
        {
            SessionId = voiceSessionId;
            VoiceServerUpdate = new VoiceServerUpdate(server);
        }
    }
}