using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria.Common;

namespace Victoria.Addon
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct LyricsSearcher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task<string> ScrapeGeniusAsync(string token, string artist, string title)
        {
            if (string.IsNullOrWhiteSpace(token))
                Throw.ArgNull(token, "Genius token cannot be null.");
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task<string> FetchFromOvhAsync(string artist, string title)
        {
            return default;
        }

        private (string Author, string Title) GetSongInfo(string trackAuthor, string trackTitle)
        {
            var split = trackTitle.Split('-');

            if (split.Length is 1)
                return (trackAuthor, trackTitle);

            var author = split[0];
            var title = split[1];
            var regex = new Regex(@"(ft).\s+\w+|\(.*?\)|(lyrics)");

            while (regex.IsMatch(title))
                title = regex.Replace(title, string.Empty);

            return author switch {
                null                              => (trackAuthor, title),
                _ when author.Equals(trackAuthor) => (trackAuthor, title),
                _                                 => (author, title)
            };
        }
    }
}
