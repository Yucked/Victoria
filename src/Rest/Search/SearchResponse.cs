using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Converters;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Victoria.Rest.Search {
    /// <summary>
    /// 
    /// </summary>
    public sealed class SearchResponse {
        private static readonly JsonSerializerOptions TrackConverterOptions = new()
        {
            Converters = {
            new LavaTrackConverter(),
            new LavaTrackListConverter(),
        }
        };
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

            if (!document.RootElement.TryGetProperty("data", out var dataElement))
            {
                return;
            }

            switch (Type)
            {
                case SearchType.Track:
                    LavaTrack track = JsonSerializer.Deserialize<LavaTrack>(dataElement, TrackConverterOptions);
                    Tracks = [track];
                    break;
                case SearchType.Playlist:
                    Exception = JsonSerializer.Deserialize<SearchException>(dataElement.GetProperty("info"));
                    Tracks = JsonSerializer.Deserialize<IReadOnlyCollection<LavaTrack>>(dataElement.GetProperty("tracks"), TrackConverterOptions);
                    break;
                case SearchType.Search:
                    Tracks = JsonSerializer.Deserialize<IReadOnlyCollection<LavaTrack>>(dataElement, TrackConverterOptions);
                    break;
                case SearchType.Error:
                    Exception = JsonSerializer.Deserialize<SearchException>(dataElement);
                    break;
            }
        }
    }
}