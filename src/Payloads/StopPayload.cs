namespace Victoria.Payloads {
	internal sealed class StopPayload : PlayerPayload {
		public StopPayload(ulong guildId) : base(guildId, "stop") {
		}
	}
}