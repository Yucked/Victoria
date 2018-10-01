using Newtonsoft.Json;

namespace Victoria.Payloads
{
    internal sealed class PausePayload : LavaPayload
    {
        [JsonProperty("pause")]
        public bool Pause { get; }

        public PausePayload(bool shouldPause, ulong id) : base("pause", id)
        {
            Pause = shouldPause;
        }
    }
}