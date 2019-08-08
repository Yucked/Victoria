namespace Victoria.Lavalink.Enums
{
    /// <summary>
    /// </summary>
    public enum LoadType
    {
        /// <summary>
        ///     Returned when a single track is loaded.
        /// </summary>
        TrackLoaded,

        /// <summary>
        ///     Returned when a playlist is loaded.
        /// </summary>
        PlaylistLoaded,

        /// <summary>
        ///     Returned when a search result is made (i.e ytsearch: some song).
        /// </summary>
        SearchResult,

        /// <summary>
        ///     Returned if no matches/sources could be found for a given identifier.
        /// </summary>
        NoMatches,

        /// <summary>
        ///     Returned if Lavaplayer failed to load something for some reason.
        /// </summary>
        LoadFailed
    }
}