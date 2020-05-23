using Victoria.Decoder;
using Victoria.Responses.WebSocket;

namespace Victoria.EventArgs {
	/// <summary>
	///     Information about track that threw an exception.
	/// </summary>
	public readonly struct TrackExceptionEventArgs {
		/// <summary>
		///     Player for which this event fired.
		/// </summary>
		public LavaPlayer Player { get; }

		/// <summary>
		///     Track sent by Lavalink.
		/// </summary>
		public LavaTrack Track { get; }

		/// <summary>
		///     Reason for why track threw an exception.
		/// </summary>
		public string ErrorMessage { get; }

		internal TrackExceptionEventArgs(LavaPlayer player, TrackExceptionEvent exceptionEvent) {
			Player = player;
			Track = TrackDecoder.Decode(exceptionEvent.Hash);
			ErrorMessage = exceptionEvent.Error;
		}
	}
}