using System.Text.Json.Serialization;

namespace Victoria.Responses.Search {
    /// <summary>
    ///     Only available if SearchStatus was PlaylistLoaded.
    /// </summary>
    public struct SearchPlaylist {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("name"), JsonInclude]
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("selectedTrack"), JsonInclude]
        public int SelectedTrack { get; private set; }
    }
}