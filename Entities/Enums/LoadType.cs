using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Victoria.Entities
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LoadType
    {
        /// <summary>
        /// Returned when a single track is loaded.
        /// </summary>
        [EnumMember(Value = "TRACK_LOADED")]
        TrackLoaded,

        /// <summary>
        /// Returned when a playlist is loaded.
        /// </summary>
        [EnumMember(Value = "PLAYLIST_LOADED")]
        PlaylistLoaded,

        /// <summary>
        /// Returned when a search result is made (i.e ytsearch: some song).
        /// </summary>
        [EnumMember(Value = "SEARCH_RESULT")]
        SearchResult,

        /// <summary>
        /// Returned if no matches/sources could be found for a given identifier.
        /// </summary>
        [EnumMember(Value = "NO_MATCHES")]
        NoMatches,

        /// <summary>
        /// Returned if Lavaplayer failed to load something for some reason.
        /// </summary>
        [EnumMember(Value = "LOAD_FAILED")]
        LoadFailed
    }
}