using System.Text.Json.Serialization;

namespace Victoria.Entities.Responses.LoadTracks
{    
    public enum LoadType
    {
        [JsonPropertyName("TRACK_LOADED")]
        TrackLoaded,

        [JsonPropertyName("PLAYLIST_LOADED")]
        PlaylistLoaded,

        [JsonPropertyName("SEARCH_RESULT")]
        SearchResult,

        [JsonPropertyName("NO_MATCHES")]
        NoMatches,

        [JsonPropertyName("LOAD_FAILED")]
        LoadFailed
    }
}