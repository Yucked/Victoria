using Victoria.Lavalink.EventArgs;

namespace Victoria.Lavalink.Responses.WebSocket
{
    internal sealed class StatsResponse : BaseWsResponse
    {
        public int Players { get; set; }
        public int PlayingPlayers { get; set; }
        public long Uptime { get; set; }

        public Cpu Cpu { get; set; }
        public Memory Memory { get; set; }
        public Frames Frames { get; set; }
    }
}