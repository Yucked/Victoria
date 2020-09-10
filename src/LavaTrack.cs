using System;

namespace Victoria {
    /// <summary>
    /// Track information.
    /// </summary>
    public class LavaTrack {
        /// <summary>
        ///     Track's author.
        /// </summary>
        public string Author { get; }

        /// <summary>
        ///     Whether the track is seekable.
        /// </summary>
        public bool CanSeek { get; }

        /// <summary>
        ///     Track's length.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        ///     Track's encoded hash.
        /// </summary>
        public string Hash { get; }

        /// <summary>
        ///     Audio / Video track Id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Whether the track is a stream.
        /// </summary>
        public bool IsStream { get; }

        /// <summary>
        ///     Track's current position.
        /// </summary>
        public TimeSpan Position { get; internal set; }

        /// <summary>
        ///     Track's title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     Track's url.
        /// </summary>
        public string Url { get; }

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
    }
}