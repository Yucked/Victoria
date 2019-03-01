using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Victoria.Entities
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrackEndReason
    {
        [EnumMember(Value = "FINISHED")]
        Finished,

        [EnumMember(Value = "LOAD_FAILED")]
        LoadFailed,

        [EnumMember(Value = "STOPPED")]
        Stopped,

        [EnumMember(Value = "REPLACED")]
        Replaced,

        [EnumMember(Value = "CLEANUP")]
        Cleanup
    }
}