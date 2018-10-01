using Newtonsoft.Json;
using Discord.WebSocket;

namespace Victoria.Objects
{
    internal sealed class VoiceServerUpdate
    {
        [JsonProperty("token")] 
        public string Token { get; }

        [JsonProperty("guild_id")] 
        public string GuildId { get; }

        [JsonProperty("endpoint")] 
        public string Endpoint { get; }

        internal VoiceServerUpdate(SocketVoiceServer server)
        {
            Token = server.Token;
            Endpoint = server.Endpoint;
            GuildId = $"{server.Guild.Id}";
        }
    }
}