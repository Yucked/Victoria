using System.Text.Json.Serialization;

namespace Victoria.Entities.Responses.LoadTracks
{
    public sealed class PlaylistInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; internal set; }

        [JsonPropertyName("selectedTracks")]
        public int SelectedTracks { get; internal set; }
    }
}