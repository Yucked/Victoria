using System;
using Newtonsoft.Json;

namespace Victoria.Entities.Payloads
{
    internal sealed class PlayPayload : LavaPayload
    {
        [JsonProperty("track")]
        public string EncryptedId { get; }

        [JsonProperty("startTime")]
        public int StartTime { get; }

        [JsonProperty("endTime")]
        public int EndTime { get; }

        [JsonProperty("noReplace")]
        public bool NoReplace { get; }

        public PlayPayload(ulong guildId, string encryptedId,
                              TimeSpan start, TimeSpan end,
                              bool noReplace) : base(guildId, "play")
        {
            EncryptedId = encryptedId;
            StartTime = (int)start.TotalMilliseconds;
            EndTime = (int)end.TotalMilliseconds;
            NoReplace = noReplace;
        }
    }
}