using Victoria.Frostbyte.Enums;

namespace Victoria.Frostbyte.Payloads
{
    internal sealed class VoiceServerPayload : BasePayload
    {
        public string SessionId { get; set; }
        public string Token { get; set; }
        public string Endpoint { get; set; }

        public VoiceServerPayload(ulong guildId) : base(OperationType.VoiceServer, guildId)
        {
        }
    }
}