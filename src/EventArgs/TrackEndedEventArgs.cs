using Victoria.Decoder;
using Victoria.Enums;
using Victoria.Responses.WebSocket;

namespace Victoria.EventArgs {
	/// <summary>
	///     Information about track that ended.
	/// </summary>
	public readonly struct TrackEndedEventArgs {
		/// <summary>
		///     Player for which this event fired.
		/// </summary>
		public LavaPlayer Player { get; }

		/// <summary>
		///     Track sent by Lavalink.
		/// </summary>
		public LavaTrack Track { get; }

		/// <summary>
		///     Reason for track ending.
		/// </summary>
		public TrackEndReason Reason { get; }

		internal TrackEndedEventArgs(LavaPlayer player, TrackEndEvent endEvent) {
			Player = player;
			Track = TrackDecoder.Decode(endEvent.Hash);
			Reason = endEvent.Reason;
		}
	}
}