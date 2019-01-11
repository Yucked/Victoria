using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Victoria.Entities;

namespace Victoria.Utilities
{
    public sealed class LyricsResolver
    {
        /// <summary>
        /// Searches lyrics for your query.
        /// </summary>
        /// <param name="searchText">Search query of your lyrics.</param>
        public static async Task<string> SearchAsync(string searchText)
        {
            var (author, title) = await SuggestAsync(searchText).ConfigureAwait(false);
            return await SearchExactAsync(author, title).ConfigureAwait(false);
        }

        /// <summary>
        /// Tries to find lyrics of the specified track.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        public static Task<string> SearchAsync(LavaTrack track)
        {
            return SearchAsync(track.Author, track.Title);
        }

        /// <summary>
        /// Tries to find lyrics.
        /// </summary>
        /// <param name="trackAuthor">Name of artist.</param>
        /// <param name="trackTitle">Name of track.</param>
        public static Task<string> SearchAsync(string trackAuthor, string trackTitle)
        {
            var (author, title) = GetSongInfo(trackAuthor, trackTitle);
            return SearchExactAsync(author, title);
        }

        private static async Task<string> MakeRequestAsync(string url)
        {
            using (var http = new HttpClient {BaseAddress = new Uri("https://api.lyrics.ovh/")})
            {
                var get = await http.GetAsync(url).ConfigureAwait(false);
                if (!get.IsSuccessStatusCode)
                    return string.Empty;

                using (var content = get.Content)
                    return await content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private static async Task<(string Author, string Title)> SuggestAsync(string searchText)
        {
            var request = await MakeRequestAsync($"suggest/{HttpUtility.UrlEncode(searchText)}").ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(request))
                return default;

            var parseRequest = JObject.Parse(request);
            if (!parseRequest.TryGetValue("total", out var total) || total.ToObject<int>() == 0)
                return default;

            parseRequest.TryGetValue("data", out var data);
            var artist = data.First.SelectToken("artist").SelectToken("name").ToObject<string>();
            var title = data.First.SelectToken("title").ToObject<string>();
            return (artist, title);
        }

        private static async Task<string> SearchExactAsync(string trackAuthor, string trackTitle)
        {
            var request =
                await MakeRequestAsync($"v1/{HttpUtility.UrlEncode(trackAuthor)}/{HttpUtility.UrlEncode(trackTitle)}")
                    .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(request))
                return default;

            var parse = JObject.Parse(request);
            if (!parse.TryGetValue("lyrics", out var result))
                return $"{parse.GetValue("error")}";

            var clean = Regex.Replace($"{result}", @"[\r\n]{2,}", "\n");
            return clean;
        }

        private static (string Author, string Title) GetSongInfo(string trackAuthor, string trackTitle)
        {
            var split = trackTitle.Split('-');
            if (split.Length is 1)
                return (trackAuthor, trackTitle);

            var author = split[0];
            var title = Regex.Replace(split[1], @" ?\(.*?\) \|(.*)", string.Empty);

            switch (author)
            {
                case "":
                case null:
                    return (trackAuthor, title);

                case var _ when string.Equals(author, trackAuthor, StringComparison.CurrentCultureIgnoreCase):
                    return (trackAuthor, title);

                default:
                    return (author, title);
            }
        }
    }
}