namespace Victoria.EventArgs {
    /// <summary>
    ///     Discord's voice websocket event.
    /// </summary>
    public struct WebSocketClosedEventArgs {
        /// <summary>
        ///     Guild's voice connection.
        /// </summary>
        public ulong GuildId { get; internal set; }

        /// <summary>
        ///     4xxx codes are bad.
        /// </summary>
        public int Code { get; internal set; }

        /// <summary>
        ///     Reason for closing websocket connection.
        /// </summary>
        public string Reason { get; internal set; }

        /// <summary>
        ///     ???
        /// </summary>
        public bool ByRemote { get; internal set; }
    }
}