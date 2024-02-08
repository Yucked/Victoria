using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Converters;

namespace Victoria {
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions {
        internal static readonly JsonSerializerOptions Options = new() {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters = {
                new LavaTrackConverter(),
                new LavaTrackListConverter(),
            }
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="requestMessage"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public static async Task<T> ReadAsJsonAsync<T>(this HttpClient httpClient, HttpRequestMessage requestMessage) {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(requestMessage);

            if (requestMessage.RequestUri == null) {
                throw new NullReferenceException(nameof(requestMessage.RequestUri));
            }

            using var responseMessage = await httpClient.SendAsync(requestMessage);
            if (!responseMessage.IsSuccessStatusCode) {
                throw new HttpRequestException(responseMessage.ReasonPhrase);
            }

            using var content = responseMessage.Content;
            await using var stream = await content.ReadAsStreamAsync();

            var deserialized = await JsonSerializer.DeserializeAsync<T>(stream);
            return deserialized;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <typeparam name="TLavaNode"></typeparam>
        /// <typeparam name="TLavaPlayer"></typeparam>
        /// <typeparam name="TLavaTrack"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddLavaNode<TLavaNode, TLavaPlayer, TLavaTrack>
            (this IServiceCollection serviceCollection)
            where TLavaNode : LavaNode<TLavaPlayer, TLavaTrack>
            where TLavaPlayer : LavaPlayer<TLavaTrack>
            where TLavaTrack : LavaTrack {
            serviceCollection.AddSingleton<Configuration>();
            serviceCollection.AddSingleton<TLavaNode>();
            return serviceCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddLavaNode(this ServiceCollection serviceCollection) {
            return AddLavaNode<
                LavaNode<LavaPlayer<LavaTrack>, LavaTrack>,
                LavaPlayer<LavaTrack>,
                LavaTrack>(serviceCollection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <typeparam name="TLavaNode"></typeparam>
        /// <typeparam name="TLavaPlayer"></typeparam>
        /// <typeparam name="TLavaTrack"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static Task UseLavaNodeAsync<TLavaNode, TLavaPlayer, TLavaTrack>
            (this IServiceProvider serviceProvider)
            where TLavaNode : LavaNode<TLavaPlayer, TLavaTrack>
            where TLavaPlayer : LavaPlayer<TLavaTrack>
            where TLavaTrack : LavaTrack {
            if (serviceProvider.GetService(typeof(TLavaNode)) is not LavaNode<TLavaPlayer, TLavaTrack> lavaNode) {
                throw new NullReferenceException(nameof(TLavaNode));
            }

            if (lavaNode.IsConnected) {
                throw new InvalidOperationException("A connection is already established with Lavalink.");
            }

            return lavaNode.ConnectAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static Task UseLavaNodeAsync(this IServiceProvider serviceProvider) {
            return UseLavaNodeAsync<
                LavaNode<LavaPlayer<LavaTrack>, LavaTrack>,
                LavaPlayer<LavaTrack>,
                LavaTrack>(serviceProvider);
        }

        internal static T AsEnum<T>(this JsonElement element) where T : struct {
            return Enum.Parse<T>(element.GetString()!, true);
        }

        internal static JsonContent AsContent<T>(this T value) {
            return JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), Options);
        }
    }
}