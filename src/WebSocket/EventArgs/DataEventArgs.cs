namespace Victoria.WebSocket.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct DataEventArgs {
        /// <summary>
        /// 
        /// </summary>
        public byte[] Data { get; }

        internal DataEventArgs(byte[] data) {
            Data = data;
        }
    }
}