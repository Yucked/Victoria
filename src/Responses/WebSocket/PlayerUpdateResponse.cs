﻿using System;
using System.Text.Json.Serialization;

namespace Victoria.Responses.WebSocket
{
    internal sealed class PlayerUpdateResponse : BaseWsResponse
    {
        [JsonPropertyName("guildId")]
        private string RawGuildId { get; set; }

        [JsonIgnore]
        public ulong GuildId
            => ulong.TryParse(RawGuildId, out var id)
                ? id
                : 0;

        [JsonPropertyName("state")]
        public PlayerState State { get; set; }
    }

    internal struct PlayerState
    {
        [JsonIgnore]
        public DateTimeOffset Time
            => DateTimeOffset.FromUnixTimeMilliseconds(LongTime);

        [JsonPropertyName("time")]
        internal long LongTime { get; set; }

        [JsonIgnore]
        public TimeSpan Position
            => TimeSpan.FromMilliseconds(LongPosition);

        [JsonPropertyName("position")]
        internal long LongPosition { get; set; }
    }
}