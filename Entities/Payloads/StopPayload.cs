namespace Victoria.Entities.Payloads
{
    internal sealed class StopPayload : PlayerPayload
    {
        public StopPayload(ulong id) : base("stop", id)
        {
        }
    }
}