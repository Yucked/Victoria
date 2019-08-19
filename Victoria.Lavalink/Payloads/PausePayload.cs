using Newtonsoft.Json;

namespace Victoria.Lavalink.Payloads
{
    internal sealed class PausePayload : PlayerPayload
    {
        [JsonProperty("pause")]
        private bool Pause { get; }

        public PausePayload(ulong guildId, bool pause) : base(guildId, "pause")
        {
            Pause = pause;
        }
    }
}
