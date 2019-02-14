namespace Victoria.Entities.Payloads
{
    internal sealed class StopPayload : LavaPayload
    {
        public StopPayload(ulong guildId) : base(guildId, "stop")
        {

        }
    }
}