using Victoria.Frostbyte.Enums;

namespace Victoria.Frostbyte.Payloads
{
    internal class BasePayload
    {
        public OperationType Op { get; set; }
        public ulong GuildId { get; set; }

        public BasePayload(OperationType op, ulong guildId)
        {
            Op = op;
            GuildId = guildId;
        }
    }
}