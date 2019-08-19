using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Victoria.Common;
using Victoria.Common.Interfaces;

namespace Victoria.Addon
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static async Task<string> GetArtworkUrlAsync(this ITrack track)
        {
            var (makeRequest, url) = track.Url switch {
                var yt when yt.Contains("youtube")    => (false, $"https://img.youtube.com/vi/{track.Id}/maxresdefault.jpg"),
                var tw when tw.Contains("twitch")     => (true, $"https://api.twitch.tv/v4/oembed?url={track.Url}"),
                var vim when vim.Contains("vimeo")    => (true, $"https://vimeo.com/api/oembed.json?url={track.Url}"),
                var sc when sc.Contains("soundcloud") => (true, $"https://soundcloud.com/oembed?url={track.Url}&format=json"),
                _                                     => (false, "https://i.imgur.com/f7Gcr7S.png")
            };

            if (!makeRequest)
                return url;

            var request = await RestClient.RequestAsync(url)
                .ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<Result>(request.Span);
            return result.Url;
        }

        private struct Result
        {
            [JsonPropertyName("thumbnail_url")]
            public string Url { get; private set; }
        }
    }
}
