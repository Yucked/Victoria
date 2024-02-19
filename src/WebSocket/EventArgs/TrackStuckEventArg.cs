namespace Victoria.WebSocket.EventArgs;

/// <summary>
/// 
/// </summary>
public struct TrackStuckEventArg {
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
    public long Threshold { get; internal init; }
}