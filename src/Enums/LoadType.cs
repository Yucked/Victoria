using System.Runtime.Serialization;

namespace Victoria.Enums
{
    /// <summary>
    /// </summary>
    public enum LoadType : byte
    {
        /// <summary>
        ///     Returned when a single track is loaded.
        /// </summary>
        [EnumMember(Value = "TRACK_LOADED")]
        TrackLoaded = 84,

        /// <summary>
        ///     Returned when a playlist is loaded.
        /// </summary>
        [EnumMember(Value = "PLAYLIST_LOADED")]
        PlaylistLoaded = 80,

        /// <summary>
        ///     Returned when a search result is made (i.e ytsearch: some song).
        /// </summary>
        [EnumMember(Value = "SEARCH_RESULT")]
        SearchResult = 83,

        /// <summary>
        ///     Returned if no matches/sources could be found for a given identifier.
        /// </summary>
        [EnumMember(Value = "NO_MATCHES")]
        NoMatches = 78,

        /// <summary>
        ///     Returned if Lavaplayer failed to load something for some reason.
        /// </summary>
        [EnumMember(Value = "LOAD_FAILED")]
        LoadFailed = 76
    }
}