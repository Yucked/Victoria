using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Victoria.Resolvers {
	/// <summary>
	///     Lyrics resolver for fetching lyrics from Genius and OVH.
	/// </summary>
	public readonly struct LyricsResolver {
		/// <summary>
		///     Searches Genius for lyrics and returns them as string.
		/// </summary>
		/// <param name="lavaTrack">
		///     <see cref="LavaTrack" />
		/// </param>
		/// <returns>
		///     <see cref="string" />
		/// </returns>
		/// <exception cref="ArgumentNullException">Throws if LavaTrack is null.</exception>
		public static async ValueTask<string> SearchGeniusAsync(LavaTrack lavaTrack) {
			if (lavaTrack == null) {
				throw new ArgumentNullException(nameof(lavaTrack));
			}

			var (author, title) = lavaTrack.GetAuthorAndTitle();
			var authorTitle = $"{author}{title}"
			   .TrimStart()
			   .TrimEnd()
			   .Replace(' ', '-');

			var url = $"https://genius.com/{authorTitle}-lyrics";
			var bytes = await GetBytesAsync(url)
			   .ConfigureAwait(false);
			return VictoriaExtensions.ParseGeniusHtml(bytes);
		}

		/// <summary>
		///     Searches OVH for lyrics and returns them as string.
		/// </summary>
		/// <param name="lavaTrack">
		///     <see cref="LavaTrack" />
		/// </param>
		/// <returns>
		///     <see cref="string" />
		/// </returns>
		/// <exception cref="ArgumentNullException">Throws if LavaTrack is null.</exception>
		public static async ValueTask<string> SearchOVHAsync(LavaTrack lavaTrack) {
			if (lavaTrack == null) {
				throw new ArgumentNullException(nameof(lavaTrack));
			}

			var (author, title) = lavaTrack.GetAuthorAndTitle();
			var url = $"https://api.lyrics.ovh/v1/{author.Encode()}/{title.Encode()}";
			var bytes = await GetBytesAsync(url)
			   .ConfigureAwait(false);

			if (bytes.Length == 0) {
				throw new Exception($"No lyrics found for {lavaTrack.Title}");
			}

			var rawJson = Encoding.UTF8.GetString(bytes);
			var parse = JObject.Parse(rawJson);
			if (!parse.TryGetValue("lyrics", out var result)) {
				return parse.GetValue("error").ToObject<string>();
			}

			var regex = new Regex(@"[\r\n]{2,}");
			return regex.Replace($"{result}", "\n");
		}

		private static async ValueTask<byte[]> GetBytesAsync(string url) {
			using var httpClient = new HttpClient();
			using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
			using var responseMessage = await httpClient.SendAsync(requestMessage)
			   .ConfigureAwait(false);
			using var content = responseMessage.Content;
			var bytes = await content.ReadAsByteArrayAsync()
			   .ConfigureAwait(false);
			return bytes;
		}
	}
}