using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class RequestPayload : LavaPayload
    {
        [JsonProperty("getAll")]
        public bool GetAll { get; set; }
        
        public RequestPayload(ulong guildId, bool getAll) : base("reqState", guildId)
        {
            GetAll = getAll;
        }
    }
}