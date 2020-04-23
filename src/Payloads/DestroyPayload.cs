namespace Victoria.Payloads {
	internal sealed class DestroyPayload : PlayerPayload {
		public DestroyPayload(ulong id) : base(id, "destroy") {
		}
	}
}