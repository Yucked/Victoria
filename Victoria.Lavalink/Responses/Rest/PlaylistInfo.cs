using System.Text.Json.Serialization;

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
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        ///     Which track was selected in playlist.
        /// </summary>
        [JsonPropertyName("selectedTrack")]
        public int SelectedTrack { get; }

        internal PlaylistInfo(string name, int selectedTrack)
        {
            Name = name;
            SelectedTrack = selectedTrack;
        }
    }
}