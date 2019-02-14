using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Victoria.Entities.Responses.LoadTracks
{
    public sealed class LoadResult
    {
        [JsonPropertyName("playlistInfo")]
        public PlaylistInfo PlaylistInfo { get; set; }

        [JsonPropertyName("loadType"), JsonEnumConverter]
        public LoadType LoadType { get; set; }

        [JsonPropertyName("tracks")]
        public IReadOnlyList<Track> Tracks { get; set; }
    }
}