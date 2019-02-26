using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class VolumePayload : LavaPayload
    {
        [JsonProperty("volume")]
        public int Volume { get; }

        public VolumePayload(ulong guildId, int volume) : base(guildId, "volume")
        {
            Volume = volume;
        }
    }
}