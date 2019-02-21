using System;
using System.Text.Json.Serialization;
using Victoria.Queue;

namespace Victoria.Entities.Responses.LoadTracks
{
    public sealed class Track : IQueueObject
    {
        [JsonPropertyName("identifier")]
        public string Id { get; internal set; }

        [JsonPropertyName("isSeekable")]
        public bool IsSeekable { get; internal set; }

        [JsonPropertyName("author")]
        public string Author { get; internal set; }

        [JsonPropertyName("isStream")]
        public bool IsStream { get; internal set; }

        public TimeSpan Position
        {
            get => new TimeSpan(TrackPosition);
            internal set => Position = value;
        }

        [JsonPropertyName("position")]
        internal long TrackPosition { get; set; }

        public TimeSpan Length
            => new TimeSpan(TrackLength);

        [JsonPropertyName("length")]
        internal long TrackLength { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; internal set; }

        [JsonPropertyName("uri")]
        public Uri Uri { get; internal set; }

        public void ResetPosition()
        {
            Position = TimeSpan.Zero;
        }
    }
}