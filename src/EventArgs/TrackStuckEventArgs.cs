using System;
using Victoria.Decoder;
using Victoria.Responses.WebSocket;

namespace Victoria.EventArgs {
	/// <summary>
	///     Information about track that got stuck.
	/// </summary>
	public readonly struct TrackStuckEventArgs {
		/// <summary>
		///     Player for which this event fired.
		/// </summary>
		public LavaPlayer Player { get; }

		/// <summary>
		///     Track sent by Lavalink.
		/// </summary>
		public LavaTrack Track { get; }

		/// <summary>
		///     How long track was stuck for.
		/// </summary>
		public TimeSpan Threshold { get; }

		internal TrackStuckEventArgs(LavaPlayer player, TrackStuckEvent stuckEvent) {
			Player = player;
			Track = TrackDecoder.Decode(stuckEvent.Hash);
			Threshold = new TimeSpan(stuckEvent.ThresholdMs);
		}
	}
}