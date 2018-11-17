namespace Victoria.Entities.Payloads
{
    internal sealed class RequestPayload : LavaPayload
    {
        /// <summary>
        /// Request player and voiceinfo for a specific guild.
        /// </summary>
        /// <param name="guildId"></param>
        public RequestPayload(ulong guildId) : base("reqState", guildId)
        {
        }
    }
}