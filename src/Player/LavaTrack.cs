using System;
using System.Text.Json.Serialization;
using Victoria.Converters;

namespace Victoria.Player {
    /// <summary>
    /// Track information.
    /// </summary>
    public class LavaTrack {
        /// <summary>
        ///     Track's encoded hash.
        /// </summary>
        public string Hash { get; internal set; }

        /// <summary>
        ///     Audio / Video track Id.
        /// </summary>
        [JsonPropertyName("identifier"), JsonInclude]
        public string Id { get; private set; }

        /// <summary>
        ///     Track's author.
        /// </summary>
        [JsonPropertyName("author"), JsonInclude]
        public string Author { get; private set; }

        /// <summary>
        ///     Track's title.
        /// </summary>
        [JsonPropertyName("title"), JsonInclude]
        public string Title { get; private set; }

        /// <summary>
        ///     Whether the track is seekable.
        /// </summary>
        [JsonPropertyName("isSeekable"), JsonInclude]
        public bool CanSeek { get; private set; }

        /// <summary>
        ///     Track's length.
        /// </summary>
        [JsonPropertyName("length"), JsonConverter(typeof(LongToTimeSpanConverter)), JsonInclude]
        public TimeSpan Duration { get; private set; }

        /// <summary>
        ///     Whether the track is a stream.
        /// </summary>
        [JsonPropertyName("isStream"), JsonInclude]
        public bool IsStream { get; private set; }

        /// <summary>
        ///     Track's current position.
        /// </summary>
        [JsonPropertyName("position"), JsonConverter(typeof(LongToTimeSpanConverter)), JsonInclude]
        public TimeSpan Position { get; private set; }

        /// <summary>
        ///     Track's url.
        /// </summary>
        [JsonPropertyName("uri"), JsonInclude]
        public string Url { get; private set; }

        /// <summary>
        /// Source of where Track was fetched from.
        /// </summary>
        [JsonPropertyName("sourceName"), JsonInclude]
        public string Source { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="author"></param>
        /// <param name="url"></param>
        /// <param name="position"></param>
        /// <param name="duration"></param>
        /// <param name="canSeek"></param>
        /// <param name="isStream"></param>
        /// <param name="source"></param>
        public LavaTrack(string hash, string id, string title, string author,
                         string url, TimeSpan position, long duration,
                         bool canSeek, bool isStream, string source) {
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Position = position;
            Duration = duration < TimeSpan.MaxValue.Ticks
                ? TimeSpan.FromMilliseconds(duration)
                : TimeSpan.MaxValue;
            CanSeek = canSeek;
            IsStream = isStream;
            Source = source;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lavaTrack"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LavaTrack(LavaTrack lavaTrack) {
            if (lavaTrack == null) {
                throw new ArgumentNullException(nameof(lavaTrack));
            }

            Hash = lavaTrack.Hash;
            Id = lavaTrack.Id;
            Title = lavaTrack.Title;
            Author = lavaTrack.Author;
            Url = lavaTrack.Url;
            Position = lavaTrack.Position;
            Duration = lavaTrack.Duration;
            CanSeek = lavaTrack.CanSeek;
            IsStream = lavaTrack.IsStream;
            Source = lavaTrack.Source;
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Only used for desserialization.")]
        public LavaTrack() { }

        internal void UpdatePosition(long position) {
            Position = TimeSpan.FromMilliseconds(position);
        }

        /// <inheritdoc />
        public override string ToString() {
            return $"{Author} {Title}";
        }
    }
}