using System;

namespace Victoria.WebSocket.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public readonly struct ErrorEventArgs {
        /// <summary>
        /// 
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; }

        internal ErrorEventArgs(Exception exception) {
            Exception = exception;
            Message = exception.Message;
        }

        internal ErrorEventArgs(string message) {
            Exception = default;
            Message = message;
        }
    }
}