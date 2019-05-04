using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;
using Victoria.Entities;
using Victoria.Helpers;

namespace Victoria
{
    public static class VictoriaExtensions
    {
        internal static Regex Compiled(string pattern)
        {
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Fetches thumbnail of the specified track.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
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
        /// Searches lyrics for the specified track.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        public static Task<string> FetchLyricsAsync(this LavaTrack track)
        {
            return LyricsHelper.SearchAsync(track.Author, track.Title);
        }

        /// <summary>
        /// Transforms a single youtube video playlist url to proper youtube url
        /// </summary>
        /// <param name="url">The youtube url to sanitize</param>
        /// <returns>The sanitized youtube url</returns>
        public static string SanitizeYoutubeUrl(this string url)
        {
            Regex regex = Compiled(@"(?:youtube(?:-nocookie)?\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})\W");
            if (regex.IsMatch(url))
            {
                string identifier = regex.Match(url).Groups[1].Value;
                return $"https://www.youtube.com/watch?v={identifier}";
            }
            else
            {
                return url;
            }
        }

        /// <summary>
        /// Checks if the <see cref="TrackEndReason"/> is Finished or LoadFailed.
        /// </summary>
        /// <param name="reason"><see cref="TrackEndReason"/></param>
        public static bool ShouldPlayNext(this TrackEndReason reason)
        {
            return reason == TrackEndReason.Finished || reason == TrackEndReason.LoadFailed;
        }

        internal static void WriteLog(this Func<LogMessage, Task> log, LogSeverity severity, string message, Exception exception = null)
        {
            if (severity > Configuration.InternalSeverity)
                return;

            var logMessage = new LogMessage(severity, nameof(Victoria), message, exception);
            log?.Invoke(logMessage);
        }
    }
}