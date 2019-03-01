using Newtonsoft.Json;
using System.Collections.Generic;

namespace Victoria.Entities
{
    public sealed class SearchResult
    {
        internal SearchResult() { }

        [JsonProperty("playlistInfo")]
        public PlaylistInfo PlaylistInfo { get; private set; }

        [JsonProperty("loadType")]
        public LoadType LoadType { get; private set; }

        [JsonIgnore]
        public IEnumerable<LavaTrack> Tracks { get; internal set; }
    }
}