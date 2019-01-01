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
        private static HttpClient http = new HttpClient
        {
            BaseAddress = new Uri("https://api.lyrics.ovh/")
        };
        
        public static async Task<string> SearchAsync(string searchText)
        {
            var info = await SuggestAsync(searchText).ConfigureAwait(false);
            Console.WriteLine(info.Author);
            return await SearchExactAsync(info.Author, info.Title).ConfigureAwait(false);
        }

        public static async Task<string> SearchAsync(LavaTrack track) => await SearchAsync(track.Author, track.Title).ConfigureAwait(false);

        public static async Task<string> SearchAsync(string trackAuthor, string trackTitle)
        {
            var info = GetSongInfo(trackAuthor, trackTitle);
            return await SearchExactAsync(info.Author, info.Title).ConfigureAwait(false);
        }

        private static async Task<(string Author, string Title)> SuggestAsync(string searchText)
        {
            using (var get = await http.GetAsync($"suggest/{HttpUtility.UrlEncode(searchText)}").ConfigureAwait(false))
            {
                if (!get.IsSuccessStatusCode)
                    return default;
                using (var content = get.Content)
                {
                    var parse = JObject.Parse(await content.ReadAsStringAsync());
                    if (!parse.TryGetValue("total", out var count) || count.ToObject<int>() == 0)
                        return default;

                    JToken song = parse["data"][0];
                    return ($"{song["artist"]["name"]}", $"{song["title"]}");
                }
            }
        }

        private static async Task<string> SearchExactAsync(string trackAuthor, string trackTitle)
        {
            using (var get = await http.GetAsync($"v1/{HttpUtility.UrlEncode(trackAuthor)}/{HttpUtility.UrlEncode(trackTitle)}").ConfigureAwait(false))
            {
                if (!get.IsSuccessStatusCode)
                    return string.Empty;
                using (var content = get.Content)
                {
                    var parse = JObject.Parse(await content.ReadAsStringAsync());
                    if (!parse.TryGetValue("lyrics", out var result))
                        return $"{parse.GetValue("error")}";

                    var clean = Regex.Replace($"{result}", @"[\r\n]{2,}", "\n");
                    return clean;
                }
            }
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