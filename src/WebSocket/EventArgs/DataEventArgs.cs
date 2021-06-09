namespace Victoria.WebSocket.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct DataEventArgs {
        /// <summary>
        /// 
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsEmpty { get; }

        internal DataEventArgs(byte[] data) {
            Data = data;
            IsEmpty = data.Length == 0;
        }
    }
}