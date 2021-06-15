using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Enums;
using Victoria.Resolvers;

namespace Victoria {
    /// <summary>
    /// Additional extension methods to make workflow easier.
    /// </summary>
    public static class VictoriaExtensions {
        private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(new HttpClient());
        internal static readonly HttpClient HttpClient = LazyHttpClient.Value;

        internal static CancellationToken DefaultTimeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="jsonConverter"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public static async Task<T> ReadAsJsonAsync<T>(HttpRequestMessage requestMessage,
                                                       JsonConverter jsonConverter = default,
                                                       CancellationToken cancellationToken = default) {
            if (requestMessage == null) {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            if (requestMessage.RequestUri == null) {
                throw new NullReferenceException(nameof(requestMessage.RequestUri));
            }

            using var responseMessage = await HttpClient.SendAsync(requestMessage, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode) {
                throw new HttpRequestException(responseMessage.ReasonPhrase);
            }

            using var content = responseMessage.Content;
            await using var stream = await content.ReadAsStreamAsync();

            var deserialized = await JsonSerializer.DeserializeAsync<T>(stream,
                jsonConverter == default
                    ? default
                    : new JsonSerializerOptions {
                        Converters = {jsonConverter}
                    }, cancellationToken);
            return deserialized;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public static async Task<JsonElement> GetJsonRootAsync(HttpRequestMessage requestMessage,
                                                               CancellationToken cancellationToken = default) {
            if (requestMessage == null) {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            if (requestMessage.RequestUri == null) {
                throw new NullReferenceException(nameof(requestMessage.RequestUri));
            }

            using var responseMessage = await HttpClient.SendAsync(requestMessage, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode) {
                throw new HttpRequestException(responseMessage.ReasonPhrase);
            }

            using var content = requestMessage.Content;
            await using var stream = await content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return document.RootElement;
        }

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
        public static ValueTask<string> FetchLyricsFromOvhAsync(this LavaTrack track) {
            return LyricsResolver.SearchOvhAsync(track);
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
    }
}