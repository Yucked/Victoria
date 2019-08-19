using System.Collections.Generic;
using Newtonsoft.Json;
using Victoria.Lavalink.Enums;

namespace Victoria.Lavalink.Responses.Rest
{
    /// <summary>
    ///     Lavalink's REST response.
    /// </summary>
    public struct SearchResponse
    {
        /// <summary>
        ///     If loadtype is a playlist then playlist info is returned.
        /// </summary>
        [JsonProperty("playlistInfo")]
        public PlaylistInfo PlaylistInfo { get; private set; }

        /// <summary>
        ///     Search load type.
        /// </summary>
        [JsonProperty("loadType")]
        public LoadType LoadType { get; private set; }

        /// <summary>
        ///     Collection of tracks returned.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<LavaTrack> Tracks { get; private set; }

        /// <summary>
        ///     If LoadType was LoadFailed then Exception is returned.
        /// </summary>
        [JsonProperty("exception")]
        public RestException Exception { get; private set; }
    }
}
