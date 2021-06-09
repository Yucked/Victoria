namespace Victoria.WebSocket.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct MessageEventArgs {
        /// <summary>
        /// 
        /// </summary>
        public byte[] Data { get; }

        internal MessageEventArgs(byte[] data) {
            Data = data;
        }
    }
}