using System.Text.Json.Serialization;
using Victoria.Enums;

namespace Victoria.Responses.WebSocket
{
    internal class BaseWsResponse
    {
        [JsonPropertyName("op")]
        public OperationType Op { get; set; }
    }
}