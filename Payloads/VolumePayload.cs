using Newtonsoft.Json;

namespace Victoria.Payloads
{
    internal sealed class VolumePayload : LavaPayload
    {
        public VolumePayload(int volume, ulong id) : base("volume", id)
        {
            Volume = volume;
        }

        [JsonProperty("volume")] 
        public int Volume { get; }
    }
}