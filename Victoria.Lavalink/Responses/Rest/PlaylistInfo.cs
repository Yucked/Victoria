using Newtonsoft.Json;

namespace Victoria.Lavalink.Responses.Rest
{
    /// <summary>
    ///     Only available if LoadType was PlaylistLoaded.
    /// </summary>
    public struct PlaylistInfo
    {
        /// <summary>
        ///     Playlist name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        ///     Which track was selected in playlist.
        /// </summary>
        [JsonProperty("selectedTrack")]
        public int SelectedTrack { get; }

        internal PlaylistInfo(string name, int selectedTrack)
        {
            Name = name;
            SelectedTrack = selectedTrack;
        }
    }
}
