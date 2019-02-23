using Newtonsoft.Json;
using System.Collections.Generic;
using Victoria.Entities.Responses;

namespace Victoria.Entities
{
    public sealed class SearchResult
    {
        internal SearchResult() { }

        [JsonProperty("playlistInfo")]
        public PlaylistInfo PlaylistInfo { get; private set; }

        [JsonProperty("loadType")]
        public string LoadType { get; private set; }

        [JsonProperty("tracks")]
        public HashSet<LavaTrack> Tracks { get; private set; }
    }
}