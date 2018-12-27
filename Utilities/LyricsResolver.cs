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

        private static (string Author, string Title) GetSongInfo(string trackAuthor, string title)
        {
            (string Author, string Song) SanitizeTitle()
            {
                var split = title.Split('-');
                if (split.Length is 1)
                    return (string.Empty, title);

                var possAuthor = split[0];
                var possTitle = split[1];

                var cleanTitle = Regex.Replace(possTitle, @" ?\(.*?\) \|(.*)", string.Empty);

                return (possAuthor, cleanTitle);
            }

            var getInfo = SanitizeTitle();
            var check = string.Equals(getInfo.Author, trackAuthor, StringComparison.CurrentCultureIgnoreCase);

            switch (getInfo.Author)
            {
                case "":
                case null:
                    return (trackAuthor, getInfo.Song);

                case var _ when check:
                    return (trackAuthor, getInfo.Song);

                default:
                    return getInfo;
            }
        }
    }
}