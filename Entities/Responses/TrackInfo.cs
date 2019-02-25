using Newtonsoft.Json;

namespace Victoria.Entities.Responses
{
    public sealed class TrackInfo
    {
        internal TrackInfo() { }

        [JsonProperty("track")]
        public string Hash { get; private set; }

        [JsonProperty("info")]
        private LavaTrack IncompleteTrack { get; set; }

        [JsonIgnore]
        public LavaTrack Track
        {
            get
            {
                IncompleteTrack.Hash = Hash;
                return IncompleteTrack;
            }
        }
    }
}