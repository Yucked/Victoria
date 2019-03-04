using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Victoria.Helpers
{
    /// <summary>
    /// Contains method for searching lyrics.
    /// </summary>
    public sealed class LyricsHelper
    {
        internal LyricsHelper() { }

        private static Regex Compiled(string pattern)
        {
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
        }

        public static async Task<string> SearchAsync(string searchText)
        {
            var (author, title) = await SuggestAsync(searchText).ConfigureAwait(false);
            return await SearchExactAsync(author, title).ConfigureAwait(false);
        }

        public static Task<string> SearchAsync(string trackAuthor, string trackTitle)
        {
            var (author, title) = GetSongInfo(trackAuthor, trackTitle);
            return SearchExactAsync(author, title);
        }

        private static Task<string> MakeRequestAsync(string url)
        {
            return HttpHelper.Instance.GetStringAsync($"https://api.lyrics.ovh/{url}");
        }

        private static async Task<(string Author, string Title)> SuggestAsync(string searchText)
        {
            var (author, s) = GetSongInfo(string.Empty, searchText);
            var request =
                await MakeRequestAsync($"suggest/{HttpUtility.UrlEncode($"{author} {s}")}")
                    .ConfigureAwait(false);

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
                return parse.GetValue("error").ToObject<string>();

            var clean = Compiled(@"[\r\n]{2,}").Replace($"{result}", "\n");
            return clean;
        }

        private static (string Author, string Title) GetSongInfo(string trackAuthor, string trackTitle)
        {
            var split = trackTitle.Split('-');

            if (split.Length is 1)
                return (trackAuthor, trackTitle);

            var author = split[0];
            var title = split[1];
            var regex = Compiled(@"(ft).\s+\w+|\(.*?\)|(lyrics)");

            while (regex.IsMatch(title))
                title = regex.Replace(title, string.Empty);

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