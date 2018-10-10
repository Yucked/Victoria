using Discord.WebSocket;
using Newtonsoft.Json;

namespace Victoria.Objects
{
    internal sealed class VoiceServerUpdate
    {
        internal VoiceServerUpdate(SocketVoiceServer server)
        {
            Token = server.Token;
            Endpoint = server.Endpoint;
            GuildId = $"{server.Guild.Id}";
        }

        [JsonProperty("token")] 
        public string Token { get; }

        [JsonProperty("guild_id")] 
        public string GuildId { get; }

        [JsonProperty("endpoint")] 
        public string Endpoint { get; }
    }
}