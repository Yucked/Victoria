using Newtonsoft.Json;

namespace Victoria.Entities
{
    public struct PlaylistInfo
    {
        [JsonProperty("name")] 
        public string Name { get; internal set; }

        [JsonProperty("selectedTrack")] 
        public int SelectedTrack { get; internal set; }
    }
}