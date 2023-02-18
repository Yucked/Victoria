using System;
using Victoria.Interfaces;

namespace Victoria.WebSocket.EventArgs;

public class PlayerUpdateEventArg<TLavaPlayer, TLavaTrack>
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