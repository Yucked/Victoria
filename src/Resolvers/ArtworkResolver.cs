using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Victoria.Resolvers {
	/// <summary>
	///     Resolver for fetching track artwork.
	/// </summary>
	public readonly struct ArtworkResolver {
		/// <summary>
		///     Fetches artwork for Youtube, Twitch, SoundCloud and Vimeo.
		/// </summary>
		/// <param name="track"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static async ValueTask<string> FetchAsync(LavaTrack track) {
			if (track == null) {
				throw new ArgumentNullException(nameof(track));
			}

			(bool shouldSearch, string requestUrl) = track.Url.ToLower() switch {
				var yt when yt.Contains("youtube")
				=> (false, $"https://img.youtube.com/vi/{track.Id}/maxresdefault.jpg"),

				var twitch when twitch.Contains("twitch")
				=> (true, $"https://api.twitch.tv/v4/oembed?url={track.Url}"),

				var sc when sc.Contains("soundcloud")
				=> (true, $"https://soundcloud.com/oembed?url={track.Url}&format=json"),

				var vim when vim.Contains("vimeo")
				=> (true, $"https://vimeo.com/api/oembed.json?url={track.Url}"),

				_ => (false, "https://raw.githubusercontent.com/Yucked/Victoria/v5/src/Logo.png")
			};

			if (!shouldSearch) {
				return requestUrl;
			}

			using var httpClient = new HttpClient();
			using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
			using var responseMessage = await httpClient.SendAsync(requestMessage)
			   .ConfigureAwait(false);
			using var content = responseMessage.Content;
			var rawJson = await content.ReadAsStringAsync()
			   .ConfigureAwait(false);
			var parsedJson = JObject.Parse(rawJson);
			return parsedJson.TryGetValue("thumbnail_url", out var thumbnail)
				? $"{thumbnail}"
				: requestUrl;
		}
	}
}