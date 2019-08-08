using System.Text.Json.Serialization;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class VolumePayload : PlayerPayload
    {
        [JsonPropertyName("volume")]
        private int Volume { get; }

        public VolumePayload(ulong guildId, int volume) : base(guildId, "volume")
        {
            Volume = volume;
        }
    }
}