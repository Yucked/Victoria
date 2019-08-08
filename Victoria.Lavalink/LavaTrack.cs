using System;
using System.Text.Json.Serialization;
using Victoria.Common.Interfaces;

namespace Victoria.Lavalink
{
    /// <summary>
    /// </summary>
    public class LavaTrack : ITrack
    {
        /// <summary>
        ///     Track's encoded hash.
        /// </summary>
        [JsonIgnore]
        public string Hash { get; private set; }

        /// <summary>
        ///     Track's author.
        /// </summary>
        public string Author { get; private set; }

        /// <summary>
        ///     Whether the track is a stream.
        /// </summary>
        [JsonPropertyName("isStream")]
        public bool IsStream { get; private set; }

        /// <summary>
        ///     Whether the track is seekable.
        /// </summary>
        [JsonPropertyName("isSeekable")]
        public bool CanSeek { get; private set; }

        [JsonPropertyName("length")]
        private long TrackDuration { get; set; }

        [JsonPropertyName("position")]
        internal long TrackPosition { get; set; }

        /// <inheritdoc />
        public string Id { get; private set; }

        /// <inheritdoc />
        public string Title { get; private set; }

        /// <inheritdoc />
        public TimeSpan Duration
            => new TimeSpan(TrackDuration);

        /// <inheritdoc />
        public TimeSpan Position
            => new TimeSpan(TrackPosition);

        /// <inheritdoc />
        public string Url { get; private set; }

        internal LavaTrack WithHash(string hash)
        {
            Hash = hash;
            return this;
        }

        internal LavaTrack WithTitle(string title)
        {
            Title = title;
            return this;
        }

        internal LavaTrack WithAuthor(string author)
        {
            Author = author;
            return this;
        }

        internal LavaTrack WithDuration(long duration)
        {
            TrackDuration = duration;
            return this;
        }

        internal LavaTrack WithId(string id)
        {
            Id = id;
            return this;
        }

        internal LavaTrack WithStream(bool isStream)
        {
            IsStream = isStream;
            return this;
        }

        internal LavaTrack WithUrl(string url)
        {
            Url = url;
            return this;
        }
    }
}