namespace Victoria.Entities.Payloads
{
    internal sealed class DestroyPayload : PlayerPayload
    {
        public DestroyPayload(ulong id) : base("destroy", id)
        {
        }
    }
}