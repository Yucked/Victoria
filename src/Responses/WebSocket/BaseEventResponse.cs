using System.Text.Json.Serialization;
using Victoria.Decoder;
using Victoria.Enums;

namespace Victoria.Responses.WebSocket
{
    internal class BaseEventResponse : BaseWsResponse
    {
        [JsonPropertyName("guildId")]
        private string RawGuildId { get; set; }

        [JsonIgnore]
        public ulong GuildId
            => ulong.TryParse(RawGuildId, out var id)
                ? id
                : 0;

        [JsonPropertyName("type")]
        public string EventType { get; set; }
    }

    internal class TrackEventResponse : BaseEventResponse
    {
        [JsonPropertyName("track")]
        private string Hash { get; set; }

        [JsonIgnore]
        public LavaTrack Track
            => TrackDecoder.Decode(Hash);
    }

    internal sealed class TrackEndEvent : TrackEventResponse
    {
        [JsonPropertyName("reason")]
        public TrackEndReason Reason { get; set; }
    }

    internal sealed class TrackExceptionEvent : TrackEventResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    internal sealed class TrackStuckEvent : TrackEventResponse
    {
        [JsonPropertyName("thresholdMs")]
        public long ThresholdMs { get; set; }
    }

    internal sealed class WebSocketClosedEvent : BaseEventResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("byRemote")]
        public bool ByRemote { get; set; }
    }
}