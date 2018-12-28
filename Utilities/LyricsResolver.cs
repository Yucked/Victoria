using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria.Entities;

namespace Victoria.Utilities
{
    public sealed class LyricsResolver
    {
        public static async Task<string> SearchAsync(LavaTrack track)
        {
            using (var http = new HttpClient
            {
                BaseAddress = new Uri("https://api.lyrics.ovh/v1/")
            })
            {
                var info = GetSongInfo(track.Author, track.Title);
                using (var get = await http.GetAsync($"{info.Author}/{info.Title}").ConfigureAwait(false))
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
        }

        private static (string Author, string Title) GetSongInfo(string trackAuthor, string trackTitle)
        {
            var split = trackTitle.Split('-');
            if (split.Length is 1)
                return (string.Empty, trackTitle);

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