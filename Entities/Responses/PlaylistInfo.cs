using Newtonsoft.Json;

namespace Victoria.Entities
{
    public sealed class PlaylistInfo
    {
        internal PlaylistInfo() { }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("selectedTrack")]
        public int SelectedTrack { get; private set; }
    }
}