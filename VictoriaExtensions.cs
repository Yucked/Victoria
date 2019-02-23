using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Victoria.Entities;
using Victoria.Helpers;

namespace Victoria
{
    public static class VictoriaExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static async Task<string> FetchThumbnailAsync(this LavaTrack track)
        {
            var url = string.Empty;

            switch ($"{track.Uri}".ToLower())
            {
                case var yt when yt.Contains("youtube"):
                    return $"https://img.youtube.com/vi/{track.Id}/maxresdefault.jpg";

                case var twich when twich.Contains("twitch"):
                    url = $"https://api.twitch.tv/v4/oembed?url={track.Uri}";
                    break;

                case var sc when sc.Contains("soundcloud"):
                    url = $"https://soundcloud.com/oembed?url={track.Uri}&format=json";
                    break;

                case var vim when vim.Contains("vimeo"):
                    url = $"https://vimeo.com/api/oembed.json?url={track.Uri}";
                    break;
            }

            var req = await HttpHelper.Instance.GetStringAsync(url);
            var parse = JObject.Parse(req);
            return !parse.TryGetValue("thumbnail_url", out var thumb)
                ? "https://i.imgur.com/YPCEUDK.gif"
                : $"{thumb}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static Task<string> FetchLyricsAsync(this LavaTrack track)
        {
            return LyricsHelper.SearchAsync(track.Author, track.Title);
        }
    }
}