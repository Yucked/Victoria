using Newtonsoft.Json;
using System;
using Victoria.Queue;

namespace Victoria.Entities.Responses
{
    public sealed class LavaTrack : IQueueObject
    {
        [JsonProperty("identifier")]
        public string Id { get; internal set; }

        [JsonProperty("isSeekable")]
        public bool IsSeekable { get; internal set; }

        [JsonProperty("author")]
        public string Author { get; internal set; }

        [JsonProperty("isStream")]
        public bool IsStream { get; internal set; }

        [JsonIgnore]
        public TimeSpan Position
        {
            get => new TimeSpan(TrackPosition);
            internal set => Position = value;
        }

        [JsonProperty("position")]
        internal long TrackPosition { get; set; }

        [JsonIgnore]
        public TimeSpan Length
            => new TimeSpan(TrackLength);

        [JsonProperty("length")]
        internal long TrackLength { get; set; }

        [JsonProperty("title")]
        public string Title { get; internal set; }

        [JsonProperty("uri")]
        public Uri Uri { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public void ResetPosition()
        {
            Position = TimeSpan.Zero;
        }
    }
}