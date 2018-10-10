using Newtonsoft.Json;

namespace Victoria.Payloads
{
    internal sealed class PausePayload : LavaPayload
    {
        public PausePayload(bool shouldPause, ulong id) : base("pause", id)
        {
            Pause = shouldPause;
        }

        [JsonProperty("pause")] 
        public bool Pause { get; }
    }
}