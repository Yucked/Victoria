using Newtonsoft.Json;

namespace Victoria.Entities
{
    public sealed class Memory
    {
        [JsonProperty("reservable")]
        public long Reservable { get; private set; }

        [JsonProperty("used")]
        public long Used { get; private set; }

        [JsonProperty("free")]
        public long Free { get; private set; }

        [JsonProperty("allocated")]
        public long Allocated { get; private set; }
    }
}