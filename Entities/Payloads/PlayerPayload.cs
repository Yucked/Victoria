namespace Victoria.Entities.Payloads
{
    internal class PlayerPayload : BasePayload
    {
        protected PlayerPayload(ulong guildId, string op) : base(op)
        {
            GuildId = guildId;
        }

        public ulong GuildId { get; }
    }
}