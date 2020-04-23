using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Enums;
using Victoria.Resolvers;

namespace Victoria {
	/// <summary>
	/// </summary>
	public static class VictoriaExtensions {
		/// <summary>
		/// Shortcut method to add Victoria to <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="serviceCollection"><see cref="IServiceProvider"/></param>
		/// <returns><see cref="IServiceCollection"/></returns>
		public static IServiceCollection AddVictoria(this IServiceCollection serviceCollection) {
			return serviceCollection
			   .AddSingleton<LavaNode>()
			   .AddSingleton<LavaConfig>();
		}

		/// <summary>
		/// Shortcut method to use <see cref="LavaNode"/> from <see cref="IServiceProvider"/>.
		/// </summary>
		/// <param name="serviceProvider"><see cref="IServiceProvider"/></param>
		/// <exception cref="NullReferenceException">Throws if <see cref="LavaNode"/> is null in <see cref="IServiceProvider"/></exception>
		public static Task UseVictoriaAsync(this IServiceProvider serviceProvider) {
			if (!(serviceProvider.GetService(typeof(LavaNode)) is LavaNode lavaNode)) {
				throw new NullReferenceException(nameof(LavaNode));
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
		/// <returns></returns>
		public static ValueTask<string> FetchArtworkAsync(this LavaTrack track) {
			return ArtworkResolver.FetchAsync(track);
		}

		/// <summary>
		///     Fetches lyrics from Genius.
		/// </summary>
		/// <param name="track"></param>
		/// <returns></returns>
		public static ValueTask<string> FetchLyricsFromGeniusAsync(this LavaTrack track) {
			return LyricsResolver.SearchGeniusAsync(track);
		}

		/// <summary>
		///     Fetches lyrics from OVH API.
		/// </summary>
		/// <param name="track"></param>
		/// <returns></returns>
		public static ValueTask<string> FetchLyricsFromOVHAsync(this LavaTrack track) {
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

			if (split.Length is 1) {
				return (lavaTrack.Author, lavaTrack.Title);
			}

			var author = split[0];
			var title = split[1];
			var regex = new Regex(@"(ft).\s+\w+|\(.*?\)|(lyrics)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

			while (regex.IsMatch(title)) {
				title = regex.Replace(title, string.Empty);
			}

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
			if (rawHtml.Contains("Genius.ads")) {
				return string.Empty;
			}

			var htmlRegex = new Regex("<[^>]*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			return htmlRegex.Replace(rawHtml, string.Empty);
		}
	}
}