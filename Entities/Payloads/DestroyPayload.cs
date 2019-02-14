namespace Victoria.Entities.Payloads
{
    internal sealed class DestroyPayload : LavaPayload
    {
        public DestroyPayload(ulong id) : base(id, "destroy")
        {
        }
    }
}