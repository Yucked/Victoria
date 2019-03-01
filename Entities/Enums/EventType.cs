using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Victoria.Entities
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum EventType
    {
        [EnumMember(Value = "TrackEndEvent")]
        TrackEnd,

        [EnumMember(Value = "TrackStuckEvent")]
        TrackStuck,

        [EnumMember(Value = "TrackExceptionEvent")]
        TrackException,

        [EnumMember(Value = "WebSocketClosedEvent")]
        WebSocketClosed
    }
}