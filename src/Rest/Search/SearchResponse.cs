using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Victoria.Rest.Search {
    /// <summary>
    /// 
    /// </summary>
    public sealed class SearchResponse {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("loadType"), JsonConverter(typeof(JsonStringEnumConverter)), JsonInclude]
        public SearchType Type { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("playlistInfo"), JsonInclude]
        public SearchPlaylist Playlist { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("exception"), JsonInclude]
        public SearchException Exception { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyCollection<LavaTrack> Tracks { get; internal set; }

        internal SearchResponse(JsonDocument document) {
            Type = document.RootElement.GetProperty("loadType").AsEnum<SearchType>();
            if (document.RootElement.TryGetProperty("playlistInfo", out var playlistElement)) {
                Playlist = playlistElement.Deserialize<SearchPlaylist>();
            }

            if (document.RootElement.TryGetProperty("exception", out var exceptionElement)) {
                Exception = exceptionElement.Deserialize<SearchException>();
            }

            if (!document.RootElement.TryGetProperty("data", out var dataElement)) {
                return;
            }

            string encoded = null;
            LavaTrack track = null;
            object pluginInfo = null;

            foreach (var property in dataElement.EnumerateObject()) {
                switch (property.Name) {
                    case "encoded":
                        encoded = property.Value.GetString();
                        break;

                    // playlist?
                    case "info":
                        switch (Type) {
                            case SearchType.Track:
                                track = property.Value.Deserialize<LavaTrack>();
                                break;
                            case SearchType.Playlist:
                                Tracks = property.Value
                                    .EnumerateArray()
                                    .Select(x => {
                                        var track = x.GetProperty("info").Deserialize<LavaTrack>();
                                        track.Hash = x.GetProperty("encoded").GetString();
                                        track.PluginInfo = x.GetProperty("pluginInfo");
                                        return track;
                                    })
                                    .ToList();
                                break;
                        }

                        break;

                    case "pluginInfo":
                        pluginInfo = property.Value;
                        break;
                }
            }

            if (track != null) {
                track.Hash = encoded;
                track.PluginInfo = pluginInfo;
                Tracks = [track];
            }
        }
    }
}