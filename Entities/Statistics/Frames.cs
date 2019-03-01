using Newtonsoft.Json;

namespace Victoria.Entities
{
    public sealed class Frames
    {
        internal Frames() { }
        /// <summary>
        /// Average frames sent per minute.
        /// </summary>
        [JsonProperty("sent")]
        public int Sent { get; private set; }

        /// <summary>
        /// Average frames nulled per minute. 
        /// </summary>
        [JsonProperty("nulled")]
        public int Nulled { get; private set; }

        /// <summary>
        /// Average frames deficit per minute.
        /// </summary>
        [JsonProperty("deficit")]
        public int Deficit { get; private set; }
    }
}