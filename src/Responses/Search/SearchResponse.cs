using System.Collections.Generic;
using System.Text.Json.Serialization;
using Victoria.Converters;

namespace Victoria.Responses.Search {
    /// <summary>
    ///     Lavalink's REST response.
    /// </summary>
    public struct SearchResponse {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("loadType"), JsonInclude, JsonConverter(typeof(LoadStatusConverter))]
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
        public LavaException Exception { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("tracks"), JsonInclude, JsonConverter(typeof(LavaTracksPropertyConverter))]
        public IReadOnlyCollection<LavaTrack> Tracks { get; private set; }
    }
}