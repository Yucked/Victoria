using Victoria.Objects.Enums;

namespace Victoria.Objects
{
    internal struct TrackFinishData
    {
        public LavaPlayer LavaPlayer { get; internal set; }
        public LavaTrack Track { get; internal set; }
        public TrackReason Reason { get; internal set; }
    }
}