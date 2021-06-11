using Discord;

namespace Victoria.Node.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct WebSocketClosedEventArg {
        /// <summary>
        /// 
        /// </summary>
        public IGuild Guild { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public int Code { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public string Reason { get; internal init; }

        /// <summary>
        /// 
        /// </summary>
        public bool ByRemote { get; internal init; }
    }
}