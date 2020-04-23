using Victoria.Decoder;
using Victoria.Responses.WebSocket;

namespace Victoria.EventArgs {
	/// <summary>
	///     Information about the track that started.
	/// </summary>
	public readonly struct TrackStartEventArgs {
		/// <summary>
		///     Player for which this event fired.
		/// </summary>
		public LavaPlayer Player { get; }

		/// <summary>
		///     Track sent by Lavalink.
		/// </summary>
		public LavaTrack Track { get; }

		internal TrackStartEventArgs(LavaPlayer player, TrackStartEvent trackStartEvent) {
			Player = player;
			Track = TrackDecoder.Decode(trackStartEvent.Hash);
		}
	}
}