using System;
using Newtonsoft.Json;

namespace Victoria.Frostbyte.EventArgs
{
    /// <summary>
    /// </summary>
    public sealed class TrackUpdateEventArgs
    {
        /// <summary>
        ///     Track's current position.
        /// </summary>
        public TimeSpan Position
            => new TimeSpan(RawPosition);

        [JsonProperty("position")]
        private long RawPosition { get; set; }
    }
}
