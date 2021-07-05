namespace Victoria.WebSocket.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct RetryEventArgs {
        /// <summary>
        /// 
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLastRetry { get; }

        internal RetryEventArgs(int count, bool isLastRetry) {
            Count = count;
            IsLastRetry = isLastRetry;
        }
    }
}