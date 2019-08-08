using System.Text.Json.Serialization;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class PausePayload : PlayerPayload
    {
        [JsonPropertyName("pause")]
        private bool Pause { get; }

        public PausePayload(ulong guildId, bool pause) : base(guildId, "pause")
        {
            Pause = pause;
        }
    }
}