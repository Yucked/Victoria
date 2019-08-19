using Victoria.Frostbyte.Enums;
using Victoria.Frostbyte.Infos;

namespace Victoria.Frostbyte.Responses
{
    internal class EventResponse
    {
        public EventType EventType { get; set; }
    }

    internal class PlayerResponse : EventResponse
    {
        public ulong GuildId { get; private set; }
        public TrackInfo Track { get; private set; }
    }
}
