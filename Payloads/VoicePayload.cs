using Discord.WebSocket;
using Newtonsoft.Json;
using Victoria.Objects;

namespace Victoria.Payloads
{
    internal sealed class VoicePayload : LavaPayload
    {
        public VoicePayload(SocketVoiceServer server, SocketVoiceState state)
            : base("voiceUpdate", server.Guild.Id)
        {
            SessionId = state.VoiceSessionId;
            VoiceServerUpdate = new VoiceServerUpdate(server);
        }

        [JsonProperty("sessionId")] 
        public string SessionId { get; }

        [JsonProperty("event")] 
        internal VoiceServerUpdate VoiceServerUpdate { get; }
    }
}