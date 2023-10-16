using System.Runtime.Serialization;

namespace Victoria.Rest.Search {
    /// <summary>
    /// Search status when searching for songs via Lavalink.
    /// </summary>
    public enum SearchType {
        /// <summary>
        /// A track has been loaded
        /// </summary>
        [EnumMember(Value = "track")]
        Track,

        /// <summary>
        /// A playlist has been loaded
        /// </summary>
        [EnumMember(Value = "playlist")]
        Playlist,

        /// <summary>
        /// A search result has been loaded
        /// </summary>
        [EnumMember(Value = "search")]
        Search,

        /// <summary>
        /// There has been no matches for your identifier
        /// </summary>
        [EnumMember(Value = "empty")]
        Empty,

        /// <summary>
        /// Loading has failed with an error
        /// </summary>
        [EnumMember(Value = "error")]
        Error
    }
}