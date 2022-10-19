using System;
using Victoria.Player;

namespace Victoria.Node.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct UpdateEventArg<TLavaPlayer, TLavaTrack>
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        /// <summary>
        /// 
        /// </summary>
        public TLavaPlayer Player { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack Track { get; internal init; }

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
}