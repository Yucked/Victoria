using System.Collections.Generic;
using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Victoria.Responses.Search {
    /// <summary>
    /// 
    /// </summary>
    public struct SearchResponse {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("loadType"), JsonInclude]
        public SearchStatus Status { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("playlistInfo"), JsonInclude]
        public SearchPlaylist Playlist { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("exception"), JsonInclude]
        public SearchException Exception { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("tracks"), JsonInclude]
        public IReadOnlyCollection<object> Tracks { get; private set; }
    }
}