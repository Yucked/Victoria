using Victoria.Responses.WebSocket;

namespace Victoria.EventArgs {
	/// <summary>
	///     Discord's voice websocket event.
	/// </summary>
	public struct WebSocketClosedEventArgs {
		/// <summary>
		///     Guild's voice connection.
		/// </summary>
		public ulong GuildId { get; }

		/// <summary>
		///     4xxx codes are bad.
		/// </summary>
		public int Code { get; }

		/// <summary>
		///     Reason for closing websocket connection.
		/// </summary>
		public string Reason { get; }

		/// <summary>
		///     ???
		/// </summary>
		public bool ByRemote { get; }

		internal WebSocketClosedEventArgs(WebSocketClosedEvent closedEvent) {
			GuildId = closedEvent.GuildId;
			Code = closedEvent.Code;
			Reason = closedEvent.Reason;
			ByRemote = closedEvent.ByRemote;
		}
	}
}