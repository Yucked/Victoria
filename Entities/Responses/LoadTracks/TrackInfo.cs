using System.Text.Json.Serialization;

namespace Victoria.Entities.Responses.LoadTracks
{
    public sealed class TrackInfo
    {
        [JsonPropertyName("track")]
        public string TrackString { get; set; }

        [JsonPropertyName("info")]
        public Track Track { get; set; }
    }
}