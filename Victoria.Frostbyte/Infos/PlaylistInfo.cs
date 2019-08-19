using System;
using Newtonsoft.Json;

namespace Victoria.Frostbyte.Infos
{
    /// <summary>
    ///     If LoadType was playlist loaded then playlist info is returned.
    /// </summary>
    public struct PlaylistInfo
    {
        /// <summary>
        ///     Playlist Id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     Playlist name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Playlist url.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        ///     How long the playlist is.
        /// </summary>
        [JsonIgnore]
        public TimeSpan Duration
            => new TimeSpan(RawDuration);

        [JsonProperty("duration")]
        private long RawDuration { get; set; }

        /// <summary>
        ///     If playlist has any artwork.
        /// </summary>
        public string ArtworkUrl { get; private set; }
    }
}
