using Discord.WebSocket;
using System.Text.Json.Serialization;

namespace Victoria.Entities
{
    internal sealed class VoiceServerUpdate
    {
        [JsonPropertyName("token")]
        public string Token { get; }

        [JsonPropertyName("guildid")]
        public string GuildId { get; }

        [JsonPropertyName("endpoint")]
        public string Endpoint { get; }

        internal VoiceServerUpdate(SocketVoiceServer server)
        {
            Token = server.Token;
            Endpoint = server.Endpoint;
            GuildId = $"{server.Guild.Id}";
        }
    }
}