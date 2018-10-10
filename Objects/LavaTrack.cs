using System;
using Newtonsoft.Json;

namespace Victoria.Objects
{
    public sealed class LavaTrack
    {
        [JsonIgnore]
        public string TrackString { get; internal set; }

        [JsonProperty("identifier")]
        public string Id { get; internal set; }

        [JsonProperty("isSeekable")]
        public bool IsSeekable { get; internal set; }

        [JsonProperty("author")]
        public string Author { get; internal set; }

        [JsonIgnore]
        public TimeSpan Length
            => !IsStream ? TimeSpan.FromMilliseconds(length) : TimeSpan.Zero;

        [JsonProperty("length")]
        internal long length { get; set; }

        [JsonProperty("isStream")]
        public bool IsStream { get; internal set; }

        [JsonIgnore]
        public TimeSpan Position
            => TimeSpan.FromMilliseconds(position);

        [JsonProperty("position")]
        internal long position { get; set; }

        [JsonProperty("title")]
        public string Title { get; internal set; }

        [JsonProperty("uri")]
        public Uri Uri { get; internal set; }
    }
}