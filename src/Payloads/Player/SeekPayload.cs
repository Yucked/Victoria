using System;
using System.Text.Json.Serialization;

namespace Victoria.Payloads.Player {
    internal record SeekPayload : AbstractPlayerPayload {
        [JsonPropertyName("position"), JsonInclude]
        private long Position { get; }

        public SeekPayload(ulong guildId, TimeSpan position) : base(guildId, "seek") {
            Position = (long) position.TotalMilliseconds;
        }
    }
}