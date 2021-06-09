using Victoria.Interfaces;

namespace Victoria.Tests.Objects {
    public sealed class CustomTrack : AbstractLavaTrack {
        public ulong Requester { get; init; }

        public CustomTrack(ILavaTrack lavaTrack) : base(lavaTrack) { }
    }
}