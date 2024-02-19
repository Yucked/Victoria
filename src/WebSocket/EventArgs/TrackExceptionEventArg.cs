using Victoria.Enums;

namespace Victoria.WebSocket.EventArgs;

/// <summary>
/// 
/// </summary>
public struct TrackExceptionEventArg {
    /// <summary>
    /// 
    /// </summary>
    public ulong GuildId { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public LavaTrack Track { get; internal init; }

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