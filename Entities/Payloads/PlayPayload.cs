using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class PlayPayload : PlayerPayload
    {
        [JsonProperty("track")] 
        public string Track { get; }

        public PlayPayload(string trackString, ulong guildId) : base("play", guildId)
        {
            Track = trackString;
        }
    }
}