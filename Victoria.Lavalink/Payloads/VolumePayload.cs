using Newtonsoft.Json;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class VolumePayload : PlayerPayload
    {
        [JsonProperty("volume")]
        private int Volume { get; }

        public VolumePayload(ulong guildId, int volume) : base(guildId, "volume")
        {
            Volume = volume;
        }
    }
}
