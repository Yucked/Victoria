using Victoria.Interfaces;

namespace Victoria.WebSocket.EventArgs;

public struct TrackExceptionEventArg<TLavaPlayer, TLavaTrack>
    where TLavaTrack : ILavaTrack
    where TLavaPlayer : ILavaPlayer<TLavaTrack> {
    /// <summary>
    /// 
    /// </summary>
    public TLavaPlayer Player { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public TLavaTrack Track { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public LavaException Exception { get; internal init; }
}