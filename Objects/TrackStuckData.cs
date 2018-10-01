namespace Victoria.Objects
{
    internal struct TrackStuckData
    {
        public LavaPlayer LavaPlayer { get; internal set; }
        public long Threshold { get; internal set; }
        public LavaTrack Track { get; internal set; }
    }
}