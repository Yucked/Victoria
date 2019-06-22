using Newtonsoft.Json;
using System;
using Victoria.Queue;

namespace Victoria.Entities
{
    public class LavaTrack : IQueueObject
    {
        [JsonIgnore]
        internal string Hash { get; set; }

        [JsonProperty("identifier")]
        public virtual string Id { get; internal set; }

        [JsonProperty("isSeekable")]
        public virtual bool IsSeekable { get; internal set; }

        [JsonProperty("author")]
        public virtual string Author { get; internal set; }

        [JsonProperty("isStream")]
        public virtual bool IsStream { get; internal set; }

        [JsonIgnore]
        public virtual TimeSpan Position
        {
            get => new TimeSpan(TrackPosition);
            internal set => TrackPosition = value.Ticks;
        }

        [JsonProperty("position")]
        internal long TrackPosition { get; set; }

        [JsonIgnore]
        public virtual TimeSpan Length
            => TimeSpan.FromMilliseconds(TrackLength);

        [JsonProperty("length")]
        internal long TrackLength { get; set; }

        [JsonProperty("title")]
        public virtual string Title { get; internal set; }

        [JsonProperty("uri")]
        public virtual Uri Uri { get; internal set; }

        [JsonIgnore]
        public virtual string Provider
        {
            get => this.Uri.GetProvider();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ResetPosition()
        {
            Position = TimeSpan.Zero;
        }
    }
}
