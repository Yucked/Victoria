using System;
using Victoria.Interfaces;

namespace Victoria
{
    /// <summary>
    /// </summary>
    public class LavaTrack : IQueueable
    {
        /// <summary>
        ///     Track's encoded hash.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        ///     Audio / Video track Id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     Track's title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        ///     Track's author.
        /// </summary>
        public string Author { get; private set; }

        /// <summary>
        ///     Whether the track is a stream.
        /// </summary>
        public bool IsStream { get; private set; }

        /// <summary>
        ///     Whether the track is seekable.
        /// </summary>
        public bool CanSeek { get; private set; }

        /// <summary>
        ///     Track's length.
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        ///     Track's current position.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        ///     Track's url.
        /// </summary>
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
            Duration = TimeSpan.FromMilliseconds(duration);
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

        internal void WithPosition(long position)
            => Position = TimeSpan.FromMilliseconds(position);

        internal void WithSeek(bool isSeekable)
            => CanSeek = isSeekable;

        /// <inheritdoc />
        public override string ToString()
            => $"Hash:{Extensions.GetWhitespace(Hash, 10)}{Hash.Substring(0, 15)}...\n" +
               $"Id:{Extensions.GetWhitespace(Id, 10)}{Id}\n" +
               $"Title:{Extensions.GetWhitespace(Title, 10)}{Title}\n" +
               $"Author:{Extensions.GetWhitespace(Author, 10)}{Author}\n" +
               $"Is Stream:{Extensions.GetWhitespace(IsStream, 10)}{IsStream}\n" +
               $"Can Seek:{Extensions.GetWhitespace(CanSeek, 10)}{CanSeek}\n" +
               $"Duration:{Extensions.GetWhitespace(Duration, 10)}{Duration}\n" +
               $"Position:{Extensions.GetWhitespace(Position, 10)}{Position}\n" +
               $"Url:{Extensions.GetWhitespace(Hash, 10)}{Url}";
    }
}