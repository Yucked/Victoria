using System;

namespace Victoria.WebSocket.EventArgs;

/// <summary>
/// 
/// </summary>
public struct PlayerUpdateEventArg {
    /// <summary>
    /// Guild id of the player
    /// </summary>
    public ulong GuildId { get; internal init; }

    /// <summary>
    /// Unix timestamp in milliseconds
    /// </summary>
    public DateTimeOffset Time { get; internal init; }

    /// <summary>
    /// The position of the track in milliseconds
    /// </summary>
    public TimeSpan Position { get; internal init; }

    /// <summary>
    /// Returns true when connected to voice gateway
    /// </summary>
    public bool IsConnected { get; internal init; }

    /// <summary>
    /// Milliseconds between heartbeat and ack
    /// </summary>
    public long Ping { get; internal init; }
}