namespace Victoria.Payloads.Player {
    internal sealed class DestroyPayload : AbstractPlayerPayload {
        public DestroyPayload(ulong id) : base(id, "destroy") { }
    }
}