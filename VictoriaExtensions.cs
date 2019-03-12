using System;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;
using Victoria.Entities;
using Victoria.Helpers;

namespace Victoria
{
    public static class VictoriaExtensions
    {
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