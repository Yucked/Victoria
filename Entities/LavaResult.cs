using System.Collections.Generic;
using Newtonsoft.Json;
using Victoria.Entities.Enums;

namespace Victoria.Entities
{
    public sealed class LavaResult
    {
        [JsonProperty("loadType")]
        public LoadResultType LoadResultType { get; internal set; }

        [JsonProperty("playlistInfo")]
        public PlaylistInfo PlaylistInfo { get; internal set; }

        [JsonIgnore]
        public IEnumerable<LavaTrack> Tracks { get; internal set; }
    }
}