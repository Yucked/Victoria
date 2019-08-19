using System;
using Newtonsoft.Json;

namespace Victoria.Lavalink.Responses.WebSocket
{
    internal sealed class PlayerUpdateResponse : BaseWsResponse
    {
        [JsonProperty("guildId")]
        private string RawGuildId { get; set; }

        [JsonIgnore]
        public ulong GuildId
            => ulong.TryParse(RawGuildId, out var id)
                ? id
                : 0;

        [JsonProperty("state")]
        public PlayerState State { get; set; }
    }

    internal struct PlayerState
    {
        [JsonIgnore]
        public DateTimeOffset Time
            => DateTimeOffset.FromUnixTimeMilliseconds(LongTime);

        [JsonProperty("time")]
        private long LongTime { get; set; }

        [JsonIgnore]
        public TimeSpan Position
            => TimeSpan.FromMilliseconds(LongPosition);

        [JsonProperty("position")]
        private long LongPosition { get; set; }
    }
}
