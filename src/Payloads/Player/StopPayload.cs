namespace Victoria.Payloads.Player {
    internal sealed record StopPayload : AbstractPlayerPayload {
        public StopPayload(ulong guildId) : base(guildId, "stop") { }
    }
}