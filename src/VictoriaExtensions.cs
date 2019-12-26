using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria.Addons;
using Victoria.Enums;

namespace Victoria {
    /// <summary>
    /// </summary>
    public static class VictoriaExtensions {
        /// <summary>
        ///     Whether the next track should be played or not.
        /// </summary>
        /// <param name="trackEndReason">Track end reason given by Lavalink.</param>
        public static bool ShouldPlayNext(this TrackEndReason trackEndReason) {
            return trackEndReason == TrackEndReason.Finished || trackEndReason == TrackEndReason.LoadFailed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static ValueTask<string> FetchThumbnailAsync(this LavaTrack track) {
            return ArtworkResolver.FetchAsync(track);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static ValueTask<string> FetchLyricsFromGenius(this LavaTrack track) {
            return LyricsResolver.SearchGeniusAsync(track);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static ValueTask<string> FetchLyricsFromOVH(this LavaTrack track) {
            return LyricsResolver.SearchOVHAsync(track);
        }

        internal static bool EnsureState(this PlayerState state) {
            return state == PlayerState.Connected
                   || state == PlayerState.Playing
                   || state == PlayerState.Paused;
        }

        internal static string Encode(this string str) {
            return WebUtility.UrlEncode(str);
        }

        internal static (string Author, string Title) GetAuthorAndTitle(this LavaTrack lavaTrack) {
            var split = lavaTrack.Title.Split('-');

            if (split.Length is 1)
                return (lavaTrack.Author, lavaTrack.Title);

            var author = split[0];
            var title = split[1];
            var regex = new Regex(@"(ft).\s+\w+|\(.*?\)|(lyrics)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            while (regex.IsMatch(title))
                title = regex.Replace(title, string.Empty);

            return author switch {
                ""                                             => (lavaTrack.Author, title),
                null                                           => (lavaTrack.Author, title),
                _ when string.Equals(author, lavaTrack.Author) => (lavaTrack.Author, title),
                _                                              => (author, title)
            };
        }

        internal static string ParseGeniusHtml(Span<byte> bytes) {
            var start = Encoding.UTF8.GetBytes("<!--sse-->");
            var end = Encoding.UTF8.GetBytes("<!--/sse-->");

            bytes = bytes.Slice(bytes.LastIndexOf(start));
            bytes = bytes.Slice(0, bytes.LastIndexOf(end));

            var rawHtml = Encoding.UTF8.GetString(bytes);
            var htmlRegex = new Regex("<[^>]*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return htmlRegex.Replace(rawHtml, string.Empty);
        }
    }
}