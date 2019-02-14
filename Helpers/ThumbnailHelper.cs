using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Victoria.Entities;

namespace Victoria.Helpers
{
    public sealed class ThumbnailHelper
    {
        /// <summary>
        /// Finds the thumbnail of <see cref="LavaTrack" />. Supports YouTube, Twitch, Soundcloud and Vimeo.
        /// </summary>
        /// <param name="track">
        /// <see cref="LavaTrack" />
        /// </param>
        /// <returns>Returns the url of the track.</returns>
        public static async Task<string> FetchAsync(LavaTrack track)
        {
            var url = string.Empty;

            switch ($"{track.Url}".ToLower())
            {
                case var yt when yt.Contains("youtube"):
                    return $"https://img.youtube.com/vi/{track.Id}/maxresdefault.jpg";

                case var twich when twich.Contains("twitch"):
                    url = $"https://api.twitch.tv/v4/oembed?url={track.Url}";
                    break;

                case var sc when sc.Contains("soundcloud"):
                    url = $"https://soundcloud.com/oembed?url={track.Url}&format=json";
                    break;

                case var vim when vim.Contains("vimeo"):
                    url = $"https://vimeo.com/api/oembed.json?url={track.Url}";
                    break;
            }

            var req = await HttpHelper.Instance.GetStringAsync(url);
            var parse = JObject.Parse(req);
            return !parse.TryGetValue("thumbnail_url", out var thumb)
                ? "https://i.imgur.com/YPCEUDK.gif"
                : $"{thumb}";
        }
    }
}