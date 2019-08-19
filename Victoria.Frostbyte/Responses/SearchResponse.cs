using System.Collections.Generic;
using Newtonsoft.Json;
using Victoria.Frostbyte.Enums;
using Victoria.Frostbyte.Infos;

namespace Victoria.Frostbyte.Responses
{
    /// <summary>
    /// </summary>
    public struct SearchResponse
    {
        /// <summary>
        /// </summary>
        [JsonProperty("status")]
        public ResponseStatus Status { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty("loadType")]
        public LoadType LoadType { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty("playlist")]
        public PlaylistInfo Playlist { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty("tracks")]
        public IEnumerable<TrackInfo> Tracks { get; set; }
    }
}
