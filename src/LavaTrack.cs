using System;
using Victoria.Interfaces;

namespace Victoria {
    /// <summary>
    /// Track information.
    /// </summary>
    public class LavaTrack : IQueueable {
        /// <summary>
        ///     Track's author.
        /// </summary>
        public string Author { get; private set; }

        /// <summary>
        ///     Whether the track is seekable.
        /// </summary>
        public bool CanSeek { get; private set; }

        /// <summary>
        ///     Track's length.
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        ///     Track's encoded hash.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        ///     Audio / Video track Id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     Whether the track is a stream.
        /// </summary>
        public bool IsStream { get; private set; }

        /// <summary>
        ///     Track's current position.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        ///     Track's title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        ///     Track's url.
        /// </summary>
        public string Url { get; private set; }

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
        public LavaTrack(string hash, string id, string title, string author,
                         string url, TimeSpan position, long duration,
                         bool canSeek, bool isStream) {
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
        }

        internal LavaTrack WithHash(string hash) {
            Hash = hash;
            return this;
        }

        internal LavaTrack WithTitle(string title) {
            Title = title;
            return this;
        }

        internal LavaTrack WithAuthor(string author) {
            Author = author;
            return this;
        }

        internal LavaTrack WithDuration(long duration) {
            Duration = duration < TimeSpan.MaxValue.Ticks
                ? TimeSpan.FromMilliseconds(duration)
                : TimeSpan.MaxValue;
            return this;
        }

        internal LavaTrack WithId(string id) {
            Id = id;
            return this;
        }

        internal LavaTrack WithStream(bool isStream) {
            IsStream = isStream;
            return this;
        }

        internal LavaTrack WithUrl(string url) {
            Url = url;
            return this;
        }

        internal void WithPosition(TimeSpan position) {
            Position = position;
        }

        internal void WithSeek(bool isSeekable) {
            CanSeek = isSeekable;
        }
    }
}