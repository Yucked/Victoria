namespace Victoria.Payloads.Player {
    internal sealed class StopPayload : AbstractPlayerPayload {
        public StopPayload(ulong guildId) : base(guildId, "stop") { }
    }
}