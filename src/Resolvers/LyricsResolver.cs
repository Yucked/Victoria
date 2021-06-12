using System;
using System.Collections.Generic;
using System.Net.Http;
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

        private static readonly Regex NewLineRegex
            = new(@"[\r\n]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string EP_OVH = "https://api.lyrics.ovh/v1/{0}/{1}";
        private const string EP_GEN = "https://genius.com/{0}-{1}-lyrics";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async ValueTask<string> SearchGeniusAsync(string artist, string title) {
            if (string.IsNullOrWhiteSpace(artist)) {
                throw new ArgumentNullException(nameof(artist));
            }

            if (string.IsNullOrWhiteSpace(title)) {
                throw new ArgumentNullException(nameof(title));
            }

            using var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, string.Format(EP_GEN, artist, title));
            var responseMessage = await Extensions.HttpClient.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode) {
                throw new HttpRequestException(responseMessage.ReasonPhrase);
            }

            using var content = responseMessage.Content;
            var responseData = await content.ReadAsByteArrayAsync();

            string ExtractJson() {
                var start = Encoding.UTF8.GetBytes("\\\"children\\\":[{\\\"children\\\":[");
                var end = Encoding.UTF8.GetBytes("\\\"\\\"],\\\"tag\\\":\\\"root\\\"}");

                Span<byte> bytes = responseData;
                bytes = bytes[bytes.LastIndexOf(start)..];
                bytes = bytes[..(bytes.LastIndexOf(end) + end.Length)];
                return Encoding.UTF8.GetString(bytes[28..^39])
                    .Replace("\'", string.Empty)
                    .Replace("\\", string.Empty);
            }

            var jsonDocument = JsonDocument.Parse(ExtractJson());
            var lyrics = new Queue<string>();

            void ExtractLyrics(IEnumerable<JsonElement> elements) {
                foreach (var element in elements) {
                    switch (element.ValueKind) {
                        case JsonValueKind.String:
                            var str = element.GetString();
                            if (string.IsNullOrWhiteSpace(str)) {
                                break;
                            }

                            lyrics.Enqueue(str);
                            break;

                        case JsonValueKind.Object:
                            if (!element.TryGetProperty("children", out var child)) {
                                break;
                            }

                            ExtractLyrics(child.EnumerateArray());
                            break;
                    }
                }
            }

            ExtractLyrics(jsonDocument.RootElement.EnumerateArray());
            return string.Join(Environment.NewLine, lyrics);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lavaTrack"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ValueTask<string> SearchOvhAsync(LavaTrack lavaTrack) {
            if (lavaTrack == null) {
                throw new ArgumentNullException(nameof(lavaTrack));
            }

            var (artist, title) = GetAuthorAndTitle(lavaTrack);
            return SearchOvhAsync(artist, title);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async ValueTask<string> SearchOvhAsync(string artist, string title) {
            if (string.IsNullOrWhiteSpace(artist)) {
                throw new ArgumentNullException(nameof(artist));
            }

            if (string.IsNullOrWhiteSpace(title)) {
                throw new ArgumentNullException(title);
            }

            using var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, string.Format(EP_OVH, artist, title));
            var jsonRoot = await Extensions.GetJsonRootAsync(requestMessage, Extensions.DefaultTimeout);

            return !jsonRoot.TryGetProperty("lyrics", out var lyricsElement)
                ? $"{jsonRoot.GetProperty("error")}"
                : NewLineRegex.Replace($"{lyricsElement}", "\n");
        }

        internal static (string Artist, string Title) GetArtistAndTitle(LavaTrack lavaTrack) {
            return default;
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