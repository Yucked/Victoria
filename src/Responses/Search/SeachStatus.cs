namespace Victoria.Responses.Search {
    /// <summary>
    /// Search status when searching for songs via Lavalink.
    /// </summary>
    public enum SearchStatus : byte {
        /// <summary>
        ///     Returned when a single track is loaded.
        /// </summary>
        TrackLoaded = 84,

        /// <summary>
        ///     Returned when a playlist is loaded.
        /// </summary>
        PlaylistLoaded = 80,

        /// <summary>
        ///     Returned when a search result is made (i.e ytsearch: some song).
        /// </summary>
        SearchResult = 83,

        /// <summary>
        ///     Returned if no matches/sources could be found for a given identifier.
        /// </summary>
        NoMatches = 78,

        /// <summary>
        ///     Returned if Lavaplayer failed to load something for some reason.
        /// </summary>
        LoadFailed = 76
    }
}