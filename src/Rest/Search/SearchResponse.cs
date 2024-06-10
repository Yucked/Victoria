using System;
using System.Collections.Generic;
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
            
            if (!document.RootElement.TryGetProperty("data", out var dataElement)) {
                return;
            }
            
            switch (Type) {
                case SearchType.Track:
                    var track = dataElement.Deserialize<LavaTrack>(Extensions.Options);
                    Tracks = [track];
                    break;
                
                case SearchType.Playlist:
                    var info = dataElement.GetProperty("info");
                    Exception = info.Deserialize<SearchException>();

                    Tracks = dataElement
                        .GetProperty("tracks")
                        .Deserialize<IReadOnlyCollection<LavaTrack>>(Extensions.Options);

                    Playlist = new SearchPlaylist(
                        info.GetProperty("name").ToString(), 
                        info.GetProperty("selectedTrack").GetInt32());
                    break;
                
                case SearchType.Search:
                    Tracks = dataElement.Deserialize<IReadOnlyCollection<LavaTrack>>(Extensions.Options);
                    break;
                
                case SearchType.Error:
                    Exception = dataElement.Deserialize<SearchException>();
                    break;
            }
        }

    }
}