using Newtonsoft.Json;

namespace Victoria.Payloads
{
    internal sealed class VolumePayload : LavaPayload
    {
        [JsonProperty("volume")]
        public int Volume { get; }
        
        public VolumePayload(int volume, ulong id) : base("volume", id)
        {
            Volume = volume;
        }
    }
}