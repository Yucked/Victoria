using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class VolumePayload : LavaPayload
    {
        [JsonProperty("volume")] 
        public int Volume { get; }
        
        public VolumePayload(int volume, ulong guildId) : base("volume", guildId)
        {
            Volume = volume;
        }
    }
}