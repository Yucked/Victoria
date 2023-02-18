using Victoria.Enums;
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
    public TrackException Exception { get; internal init; }
}

/// <summary>
/// 
/// </summary>
/// <param name="Message"></param>
/// <param name="Severity"></param>
/// <param name="Cause"></param>
public readonly record struct TrackException(
    string Message,
    ExceptionSeverity Severity,
    string Cause);