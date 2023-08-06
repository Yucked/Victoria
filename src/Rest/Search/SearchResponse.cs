using System.Collections.Generic;
using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Victoria.Rest.Search {
    /// <summary>
    /// 
    /// </summary>
    public struct SearchResponse {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("loadType"), JsonConverter(typeof(JsonStringEnumConverter))]
        public SearchStatus Status { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("playlistInfo")]
        public SearchPlaylist Playlist { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("exception")]
        public SearchException Exception { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("tracks")]
        public IReadOnlyCollection<object> Tracks { get; }
    }
}