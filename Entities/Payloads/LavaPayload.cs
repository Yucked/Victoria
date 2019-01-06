using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal abstract class LavaPayload
    {
        [JsonProperty("op")] 
        public string Operation { get; set; }

        protected LavaPayload(string op)
        {
            Operation = op;
        }
    }
}