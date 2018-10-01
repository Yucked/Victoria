namespace Victoria.Objects
{
    internal struct TrackExceptionData
    {
        public string Error { get; internal set; }
        public LavaPlayer LavaPlayer { get; internal set; }
        public LavaTrack Track { get; internal set; }
    }
}