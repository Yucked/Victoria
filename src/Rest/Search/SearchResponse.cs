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

            Tracks = dataElement.EnumerateArray()
                .AsParallel()
                .Select(x => {
                    var track = x.GetProperty("info").Deserialize<LavaTrack>();
                    track.Hash = x.GetProperty("encoded").GetString();
                    track.PluginInfo = x.GetProperty("pluginInfo");
                    return track;
                })
                .ToList();
        }
    }
}