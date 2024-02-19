using System.Threading.Tasks;
using Victoria.Enums;

namespace Victoria.WebSocket.EventArgs;

/// <summary>
/// 
/// </summary>
public struct TrackEndEventArg {
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
    public TrackEndReason Reason { get; internal init; }
}