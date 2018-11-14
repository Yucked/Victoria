namespace Victoria.Entities.Payloads
{
    internal sealed class StopPayload : LavaPayload
    {
        public StopPayload(ulong id) : base("stop", id)
        {
        }
    }
}