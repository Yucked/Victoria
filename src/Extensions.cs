using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Interfaces;

namespace Victoria {
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions {
        internal static readonly Random Random = new();
        internal static readonly HttpClient HttpClient = new();

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
            if (httpClient == null) {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (requestMessage == null) {
                throw new ArgumentNullException(nameof(requestMessage));
            }

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
        /// <param name="action"></param>
        /// <typeparam name="TLavaNode"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddLavaNode<TLavaNode>(
            this IServiceCollection serviceCollection,
            Action<NodeConfiguration> action = default)
            where TLavaNode : AbstractLavaNode, ILavaNode {
            var nodeConfiguration = new NodeConfiguration();
            action?.Invoke(nodeConfiguration);
            serviceCollection.AddSingleton(nodeConfiguration);
            serviceCollection.AddSingleton<TLavaNode>();
            return serviceCollection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static ValueTask UseLavaNodeAsync(this IServiceProvider serviceProvider) {
            if (!(serviceProvider.GetService(typeof(ILavaNode)) is ILavaNode lavaNode)) {
                throw new NullReferenceException(nameof(ILavaNode));
            }

            if (lavaNode.IsConnected) {
                throw new InvalidOperationException("A connection is already established with Lavalink.");
            }

            return lavaNode.ConnectAsync();
        }
    }
}