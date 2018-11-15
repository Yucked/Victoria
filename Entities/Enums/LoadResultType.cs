using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Victoria.Entities.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LoadResultType
    {
        [EnumMember(Value = "TRACK_LOADED")] 
        TrackLoaded,

        [EnumMember(Value = "PLAYLIST_LOADED")]
        PlaylistLoaded,

        [EnumMember(Value = "SEARCH_RESULT")] 
        SearchResult,

        [EnumMember(Value = "NO_MATCHES")] 
        NoMatches,

        [EnumMember(Value = "LOAD_FAILED")] 
        LoadFailed
    }
}