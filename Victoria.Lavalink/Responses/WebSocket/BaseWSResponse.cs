using System.Text.Json.Serialization;

namespace Victoria.Lavalink.Responses.WebSocket
{
    internal class BaseWsResponse
    {
        [JsonPropertyName("op")]
        public string Op { get; set; }
    }
}