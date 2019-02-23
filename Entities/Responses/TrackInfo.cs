using Newtonsoft.Json;

namespace Victoria.Entities.Responses
{
    public sealed class TrackInfo
    {
        internal TrackInfo() { }

        [JsonProperty("track")]
        public string Encrypted { get; private set; }

        [JsonProperty("info")]
        public LavaTrack Track { get; private set; }
    }
}