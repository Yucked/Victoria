using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Victoria.Responses.Search {
    /// <summary>
    /// 
    /// </summary>
    public struct SearchPlaylist {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("name"), JsonInclude]
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("selectedTrack"), JsonInclude]
        public int SelectedTrack { get; private set; }
    }
}