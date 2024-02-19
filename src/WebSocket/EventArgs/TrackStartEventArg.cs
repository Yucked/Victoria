namespace Victoria.WebSocket.EventArgs;

/// <summary>
/// 
/// </summary>
public struct TrackStartEventArg {
    /// <summary>
    /// 
    /// </summary>
    public ulong GuildId { get; internal init; }

    /// <summary>
    /// 
    /// </summary>
    public LavaTrack Track { get; internal init; }
}