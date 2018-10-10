using Newtonsoft.Json;

namespace Victoria.Objects
{
    public struct PlaylistInfo
    {
        [JsonProperty("name")] 
        public string Name { get; internal set; }

        [JsonProperty("selectedTrack")] 
        public int SelectedTrack { get; internal set; }
    }
}