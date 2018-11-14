using Discord.WebSocket;
using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class VoicePayload : LavaPayload
    {        
        [JsonProperty("sessionId")] 
        public string SessionId { get; }

        [JsonProperty("event")] 
        internal VoiceServerUpdate VoiceServerUpdate { get; }
        
        public VoicePayload(SocketVoiceServer server, SocketVoiceState state)
            : base("voiceUpdate", server.Guild.Id)
        {
            SessionId = state.VoiceSessionId;
            VoiceServerUpdate = new VoiceServerUpdate(server);
        }       
    }
}