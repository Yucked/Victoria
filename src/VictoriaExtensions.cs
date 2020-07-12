using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Enums;
using Victoria.Resolvers;

namespace Victoria {
    /// <summary>
    /// Additional extension methods to make workflow easier.
    /// </summary>
    public static class VictoriaExtensions {
        private static readonly Regex TitleRegex
            = new Regex(@"(ft).\s+\w+|\(.*?\)|(lyrics)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        ///     Shortcut method to add Victoria to <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">
        ///     <see cref="IServiceProvider" />
        /// </param>
        /// <param name="action">LavaConfig action.</param>
        /// <returns>
        ///     <see cref="IServiceCollection" />
        /// </returns>
        public static IServiceCollection AddLavaNode(this IServiceCollection serviceCollection,
                                                     Action<LavaConfig> action = default) {
            var lavaConfig = new LavaConfig();
            action?.Invoke(lavaConfig);
            serviceCollection.AddSingleton(lavaConfig);
            serviceCollection.AddSingleton<LavaNode>();
            return serviceCollection;
        }

        /// <summary>
        ///     Shortcut method to use <see cref="LavaNode" /> from <see cref="IServiceProvider" />.
        /// </summary>
        /// <param name="serviceProvider">
        ///     <see cref="IServiceProvider" />
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     Throws if <see cref="LavaNode" /> is null in <see cref="IServiceProvider" />
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Throws if <seealso cref="LavaNode{TPlayer}.IsConnected"/> is set to true.
        /// </exception>
        public static Task UseLavaNodeAsync(this IServiceProvider serviceProvider) {
            if (!(serviceProvider.GetService(typeof(LavaNode)) is LavaNode lavaNode)) {
                throw new NullReferenceException(nameof(LavaNode));
            }

            if (lavaNode.IsConnected) {
                throw new InvalidOperationException("A connection is already established with Lavalink.");
            }

            return lavaNode.ConnectAsync();
        }

        /// <summary>
        ///     Whether the next track should be played or not.
        /// </summary>
        /// <param name="trackEndReason">Track end reason given by Lavalink.</param>
        public static bool ShouldPlayNext(this TrackEndReason trackEndReason) {
            return trackEndReason == TrackEndReason.Finished || trackEndReason == TrackEndReason.LoadFailed;
        }

        /// <summary>
        ///     Fetches artwork for Youtube, Twitch, SoundCloud and Vimeo.
        /// </summary>
        /// <param name="track"></param>
        /// <returns><see cref="string"/></returns>
        public static ValueTask<string> FetchArtworkAsync(this LavaTrack track) {
            return ArtworkResolver.FetchAsync(track);
        }

        /// <summary>
        ///     Fetches lyrics from Genius.
        /// </summary>
        /// <param name="track"></param>
        /// <returns><see cref="string"/></returns>
        public static ValueTask<string> FetchLyricsFromGeniusAsync(this LavaTrack track) {
            return LyricsResolver.SearchGeniusAsync(track);
        }

        /// <summary>
        ///     Fetches lyrics from OVH API.
        /// </summary>
        /// <param name="track"></param>
        /// <returns><see cref="string"/></returns>
        public static ValueTask<string> FetchLyricsFromOVHAsync(this LavaTrack track) {
            return LyricsResolver.SearchOVHAsync(track);
        }

        internal static bool TryRead(this ref Utf8JsonReader reader, string content) {
            return reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals(content) && reader.Read();
        }

        internal static bool TryDeserialize<T>(this byte[] data, out T value,
                                               JsonSerializerOptions serializerOptions = default) {
            try {
                value = JsonSerializer.Deserialize<T>(data, serializerOptions);
                return true;
            }
            catch {
                value = default;
                return false;
            }
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

            if (split.Length is 1) {
                return (lavaTrack.Author, lavaTrack.Title);
            }

            var author = split[0];
            var title = split[1];

            while (TitleRegex.IsMatch(title)) {
                title = TitleRegex.Replace(title, string.Empty);
            }

            title = title.TrimStart().TrimEnd();
            return author switch {
                ""                                             => (lavaTrack.Author, title),
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
            if (rawHtml.Contains("Genius.ads")) {
                return string.Empty;
            }

            var htmlRegex = new Regex("<[^>]*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return htmlRegex.Replace(rawHtml, string.Empty).TrimStart().TrimEnd();
        }
    }
}