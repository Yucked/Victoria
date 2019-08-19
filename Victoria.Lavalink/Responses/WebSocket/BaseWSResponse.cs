using Newtonsoft.Json;

namespace Victoria.Lavalink.Responses.WebSocket
{
    internal class BaseWsResponse
    {
        [JsonProperty("op")]
        public string Op { get; set; }
    }
}
