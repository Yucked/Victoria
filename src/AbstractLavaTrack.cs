using System;
using Victoria.Interfaces;

namespace Victoria {
    /// <summary>
    /// 
    /// </summary>
    public class AbstractLavaTrack : ILavaTrack {
        /// <inheritdoc />
        public string Hash { get; }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Title { get; }

        /// <inheritdoc />
        public string Author { get; }

        /// <inheritdoc />
        public string Url { get; }

        /// <inheritdoc />
        public TimeSpan Position { get; }

        /// <inheritdoc />
        public TimeSpan Duration { get; }

        /// <inheritdoc />
        public bool IsSeekable { get; }

        /// <inheritdoc />
        public bool IsLiveStream { get; }

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
        /// <param name="isSeekable"></param>
        /// <param name="isLiveStream"></param>
        public AbstractLavaTrack(string hash, string id, string title, string author,
                                 string url, TimeSpan position, TimeSpan duration,
                                 bool isSeekable, bool isLiveStream) {
            Hash = hash;
            Id = id;
            Title = title;
            Author = author;
            Url = url;
            Position = position;
            Duration = duration;
            IsSeekable = isSeekable;
            IsLiveStream = isLiveStream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lavaTrack"></param>
        protected AbstractLavaTrack(ILavaTrack lavaTrack) {
            Hash = lavaTrack.Hash;
            Id = lavaTrack.Id;
            Title = lavaTrack.Title;
            Author = lavaTrack.Author;
            Url = lavaTrack.Url;
            Position = lavaTrack.Position;
            Duration = lavaTrack.Duration;
            IsSeekable = lavaTrack.IsSeekable;
            IsLiveStream = lavaTrack.IsLiveStream;
        }
    }
}