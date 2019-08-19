namespace Victoria.Frostbyte.Enums
{
    /// <summary>
    /// Track load information.
    /// </summary>
    public enum LoadType
    {
        /// <summary>
        /// Failed to find any matches for your query.
        /// </summary>
        NoMatches = 0,

        /// <summary>
        /// Something went wrong on Frostbyte side.
        /// </summary>
        SearchError = 1,

        /// <summary>
        /// Results for your search query.
        /// </summary>
        SearchResult = 2,

        /// <summary>
        /// When a single track is loaded.
        /// </summary>
        TrackLoaded = 3,

        /// <summary>
        /// When a requested playlist is loaded.
        /// </summary>
        PlaylistLoaded = 4
    }
}