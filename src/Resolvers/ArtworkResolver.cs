using System;
using System.Text.Json;
using System.Threading.Tasks;
using Victoria.Player;

namespace Victoria.Resolvers {
    /// <summary>
    ///     Resolver for fetching track artwork.
    /// </summary>
    public readonly struct ArtworkResolver {
        /// <summary>
        ///     Fetches artwork for Youtube, Twitch, SoundCloud and Vimeo.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async ValueTask<string> FetchAsync(LavaTrack track) {
            if (track == null) {
                throw new ArgumentNullException(nameof(track));
            }

            var (shouldSearch, requestUrl) = track.Url.ToLower() switch {
                var yt when yt.Contains("youtube")
                    => (false, $"https://img.youtube.com/vi/{track.Id}/maxresdefault.jpg"),

                var twitch when twitch.Contains("twitch")
                    => (true, $"https://api.twitch.tv/v4/oembed?url={track.Url}"),

                var sc when sc.Contains("soundcloud")
                    => (true, $"https://soundcloud.com/oembed?url={track.Url}&format=json"),

                var vim when vim.Contains("vimeo")
                    => (false, $"https://i.vimeocdn.com/video/{track.Id}.png"),

                _ => (false, "https://raw.githubusercontent.com/Yucked/Victoria/v5/src/Logo.png")
            };

            if (!shouldSearch) {
                return requestUrl;
            }

            var responseMessage = await VictoriaExtensions.HttpClient.GetAsync(requestUrl);
            if (!responseMessage.IsSuccessStatusCode) {
                throw new Exception(responseMessage.ReasonPhrase);
            }

            using var content = responseMessage.Content;
            await using var stream = await content.ReadAsStreamAsync();

            var document = await JsonDocument.ParseAsync(stream);
            return document.RootElement.TryGetProperty("thumbnail_url", out var url)
                ? $"{url}"
                : requestUrl;
        }
    }
}