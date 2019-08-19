using Newtonsoft.Json;
using Victoria.Lavalink.Decoder;
using Victoria.Lavalink.Enums;

namespace Victoria.Lavalink.Responses.WebSocket
{
    internal class BaseEventResponse : BaseWsResponse
    {
        [JsonProperty("guildId")]
        private string RawGuildId { get; set; }

        [JsonIgnore]
        public ulong GuildId
            => ulong.TryParse(RawGuildId, out var id)
                ? id
                : 0;

        [JsonProperty("type")]
        public string EventType { get; set; }
    }

    internal class TrackEventResponse : BaseEventResponse
    {
        [JsonProperty("track")]
        private string Hash { get; set; }

        [JsonIgnore]
        public LavaTrack Track
            => TrackDecoder.Decode(Hash);
    }

    internal sealed class TrackEndEvent : TrackEventResponse
    {
        [JsonProperty("reason")]
        public TrackEndReason Reason { get; set; }
    }

    internal sealed class TrackExceptionEvent : TrackEventResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }

    internal sealed class TrackStuckEvent : TrackEventResponse
    {
        [JsonProperty("thresholdMs")]
        public long ThresholdMs { get; set; }
    }

    internal sealed class WebSocketClosedEvent : BaseEventResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("byRemote")]
        public bool ByRemote { get; set; }
    }
}
