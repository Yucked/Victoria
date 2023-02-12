using System;

namespace Victoria.WebSocket {
    /// <summary>
    /// 
    /// </summary>
    public sealed class WebSocketConfiguration {
        /// <summary>
        ///     How many reconnect attempts are allowed.
        /// </summary>
        public int ReconnectAttempts { get; set; }
            = 10;

        /// <summary>
        ///     Reconnection delay for retrying websocket connection.
        /// </summary>
        public TimeSpan ReconnectDelay { get; set; }
            = TimeSpan.FromSeconds(3);

        internal string Endpoint { get; set; }
    }
}