using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria.Player;

namespace Victoria.Resolvers {
    /// <summary>
    ///     Lyrics resolver for fetching lyrics from Genius and OVH.
    /// </summary>
    public readonly struct LyricsResolver {
        private static readonly Regex TitleRegex
            = new(@"(ft).\s+\w+|\(.*?\)|(lyrics)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        ///     Searches Genius for lyrics and returns them as string.
        /// </summary>
        /// <param name="lavaTrack">
        ///     <see cref="LavaTrack" />
        /// </param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        /// <exception cref="ArgumentNullException">Throws if LavaTrack is null.</exception>
        public static async ValueTask<string> SearchGeniusAsync(LavaTrack lavaTrack) {
            if (lavaTrack == null) {
                throw new ArgumentNullException(nameof(lavaTrack));
            }

            var (author, title) = GetAuthorAndTitle(lavaTrack);
            var authorTitle = $"{author}{title}"
                .TrimStart()
                .TrimEnd()
                .Replace(' ', '-');

            var responseMessage = await Extensions.HttpClient.GetAsync($"https://genius.com/{authorTitle}-lyrics");
            if (!responseMessage.IsSuccessStatusCode) {
                throw new Exception("");
            }

            using var content = responseMessage.Content;
            var responseData = await content.ReadAsByteArrayAsync();

            string ParseGeniusHtml() {
                var start = Encoding.UTF8.GetBytes("<!--sse-->");
                var end = Encoding.UTF8.GetBytes("<!--/sse-->");

                Span<byte> bytes = responseData;
                bytes = bytes[bytes.LastIndexOf(start)..];
                bytes = bytes[..bytes.LastIndexOf(end)];

                var rawHtml = Encoding.UTF8.GetString(bytes);
                if (rawHtml.Contains("Genius.ads")) {
                    return string.Empty;
                }

                var htmlRegex = new Regex("<[^>]*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return htmlRegex.Replace(rawHtml, string.Empty).TrimStart().TrimEnd();
            }

            return ParseGeniusHtml();
        }

        /// <summary>
        ///     Searches OVH for lyrics and returns them as string.
        /// </summary>
        /// <param name="lavaTrack">
        ///     <see cref="LavaTrack" />
        /// </param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        /// <exception cref="ArgumentNullException">Throws if LavaTrack is null.</exception>
        public static async ValueTask<string> SearchOvhAsync(LavaTrack lavaTrack) {
            if (lavaTrack == null) {
                throw new ArgumentNullException(nameof(lavaTrack));
            }

            var (author, title) = GetAuthorAndTitle(lavaTrack);
            var responseMessage =
                await Extensions.HttpClient.GetAsync($"https://api.lyrics.ovh/v1/{author.Encode()}/{title.Encode()}");

            if (!responseMessage.IsSuccessStatusCode) {
                throw new Exception("");
            }

            using var content = responseMessage.Content;
            var responseStream = await content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(responseStream);
            if (!document.RootElement.TryGetProperty("lyrics", out var jsonElement)) {
                return document.RootElement.GetProperty("error").GetString();
            }

            var regex = new Regex(@"[\r\n]{2,}");
            return regex.Replace($"{jsonElement}", "\n");
        }

        internal static (string Author, string Title) GetAuthorAndTitle(LavaTrack lavaTrack) {
            var split = lavaTrack.Title.Split('-');

            if (split.Length is 1) {
                return (lavaTrack.Author, lavaTrack.Title);
            }

            var author = split[0];
            var title = split[1];

            while (TitleRegex.IsMatch(title)) {
                title = TitleRegex.Replace(title, string.Empty);
            }

            title = title.TrimStart().TrimEnd();
            return author switch {
                ""                                             => (lavaTrack.Author, title),
                _ when string.Equals(author, lavaTrack.Author) => (lavaTrack.Author, title),
                _                                              => (author, title)
            };
        }
    }
}