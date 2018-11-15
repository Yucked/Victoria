using Newtonsoft.Json;

namespace Victoria.Entities.Stats
{
    public struct Memory
    {        
        [JsonProperty("reservable")] 
        public long Reservable { get; set; }

        [JsonProperty("used")] 
        public long Used { get; set; }

        [JsonProperty("free")] 
        public long Free { get; set; }

        [JsonProperty("allocated")] 
        public long Allocated { get; set; }
    }
}