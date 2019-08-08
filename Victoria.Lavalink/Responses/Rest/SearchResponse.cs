using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Lavalink.Enums;

namespace Victoria.Lavalink.Responses.Rest
{
    /// <summary>
    ///     Lavalink's REST response.
    /// </summary>
    public readonly struct SearchResponse
    {
        /// <summary>
        ///     If loadtype is a playlist then playlist info is returned.
        /// </summary>
        [JsonPropertyName("playlistInfo")]
        public PlaylistInfo PlaylistInfo { get; }

        /// <summary>
        ///     Search load type.
        /// </summary>
        [JsonPropertyName("loadType")]
        public LoadType LoadType { get; }

        /// <summary>
        ///     Collection of tracks returned.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<LavaTrack> Tracks { get; }

        /// <summary>
        ///     If LoadType was LoadFailed then Exception is returned.
        /// </summary>
        [JsonPropertyName("exception")]
        public RestException Exception { get; }

        internal SearchResponse(JsonElement element)
        {
            PlaylistInfo = default;
            LoadType = default;
            Tracks = default;
            Exception = default;

            var loadElement = element.GetProperty("loadType");
            LoadType = ConvertToLoadType(loadElement);

            var playlistElement = element.GetProperty("playlistInfo");
            PlaylistInfo = ConvertToPlaylist(playlistElement);

            var tracksElement = element.GetProperty("tracks");
            Tracks = ConvertToTracks(tracksElement);

            if (element.TryGetProperty("exception", out var exeptionElement))
                Exception = ConvertToException(exeptionElement);
        }

        private LoadType ConvertToLoadType(JsonElement element)
        {
            var raw = element.GetString();
            return raw switch
            {
                "TRACK_LOADED"    => LoadType.TrackLoaded,
                "PLAYLIST_LOADED" => LoadType.PlaylistLoaded,
                "SEARCH_RESULT"   => LoadType.SearchResult,
                "NO_MATCHES"      => LoadType.NoMatches,
                "LOAD_FAILED"     => LoadType.LoadFailed,
                _                 => LoadType.LoadFailed
            };
        }

        private PlaylistInfo ConvertToPlaylist(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return default;

            var result = element.TryGetProperty("name", out var nameElm);
            result = element.TryGetProperty("selectedTrack", out var trackElm);

            return !result ? default : new PlaylistInfo(nameElm.GetString(), trackElm.GetInt32());
        }

        private IEnumerable<LavaTrack> ConvertToTracks(JsonElement element)
            => from obj in element.EnumerateArray()
               let hash = obj.GetProperty("track")
                   .GetString()
               let infoElm = obj.GetProperty("info")
               select new LavaTrack()
                   .WithHash(hash)
                   .WithId(infoElm.GetProperty("identifier")
                       .GetString())
                   .WithAuthor(infoElm.GetProperty("isSeekable")
                       .GetString())
                   .WithDuration(infoElm.GetProperty("length")
                       .GetInt64())
                   .WithStream(infoElm.GetProperty("isStream")
                       .GetBoolean())
                   .WithTitle(infoElm.GetProperty("title")
                       .GetString())
                   .WithUrl(infoElm.GetProperty("uri")
                       .GetString());

        private RestException ConvertToException(JsonElement element)
        {
            var message = element.GetProperty("message")
                .GetString();
            var severity = element.GetProperty("severity")
                .GetString();

            return new RestException(message, severity);
        }
    }
}
