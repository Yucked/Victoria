using System;
using Newtonsoft.Json;
using Victoria.Common.Interfaces;

namespace Victoria.Frostbyte.Infos
{
    /// <summary>
    /// Represents a track object sent by Frostbyte.
    /// </summary>
    public struct TrackInfo : ITrack
    {
        /// <summary>
        ///     Audio / Video track Id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     Track's title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        ///     Whether the track is streamable.
        /// </summary>
        public bool CanStream { get; private set; }


        /// <summary>
        ///     Track's length.
        /// </summary>
        [JsonIgnore]
        public TimeSpan Duration
            => new TimeSpan(RawDuration);

        /// <summary>
        ///     Track's current position.
        /// </summary>
        [JsonIgnore]
        public TimeSpan Position
            => new TimeSpan(RawPosition);

        /// <summary>
        ///     Track's url.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        ///     Track's provider. Eg: YouTube/SoundCloud/etc.
        /// </summary>
        public string Provider { get; private set; }

        /// <summary>
        ///     Track's artwork.
        /// </summary>
        public string ArtworkUrl { get; private set; }

        /// <summary>
        ///     Track author.
        /// </summary>
        public AuthorInfo Author { get; private set; }

        [JsonProperty("duration")]
        private long RawDuration { get; set; }

        [JsonProperty("position")]
        private long RawPosition { get; set; }
    }
}
