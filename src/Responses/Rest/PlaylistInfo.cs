namespace Victoria.Responses.Rest {
	/// <summary>
	///     Only available if LoadStatus was PlaylistLoaded.
	/// </summary>
	public struct PlaylistInfo {
		/// <summary>
		///     Playlist name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///     Which track was selected in playlist.
		/// </summary>
		public int SelectedTrack { get; private set; }

		internal void WithName(string name) {
			Name = name;
		}

		internal void WithTrack(int track) {
			SelectedTrack = track;
		}
	}
}