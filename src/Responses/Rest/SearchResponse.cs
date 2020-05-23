using System.Collections.Generic;
using Victoria.Enums;

namespace Victoria.Responses.Rest {
	/// <summary>
	///     Lavalink's REST response.
	/// </summary>
	public struct SearchResponse {
		/// <summary>
		///     Search load type.
		/// </summary>
		public LoadStatus LoadStatus { get; internal set; }

		/// <summary>
		///     If loadtype is a playlist then playlist info is returned.
		/// </summary>
		public PlaylistInfo Playlist { get; internal set; }

		/// <summary>
		///     Collection of tracks returned.
		/// </summary>
		public IReadOnlyList<LavaTrack> Tracks { get; internal set; }

		/// <summary>
		///     If LoadStatus was LoadFailed then Exception is returned.
		/// </summary>
		public RestException Exception { get; internal set; }
	}
}