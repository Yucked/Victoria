using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed partial class RequestPayload : LavaPayload
    {
        /// <summary>
        /// Request player and voiceinfo for a specific guild.
        /// </summary>
        /// <param name="guildId"></param>
        public RequestPayload(ulong guildId) : base("reqState", guildId)
        {
        }
    }

    internal sealed partial class RequestPayload
    {
        [JsonProperty("getAll")]
        public bool GetAll { get; set; }

        /// <summary>
        /// Request all players voiceinfo.
        /// </summary>
        /// <param name="getAll"></param>
        public RequestPayload(bool getAll) : base("reqState")
        {
            GetAll = getAll;
        }
    }
}