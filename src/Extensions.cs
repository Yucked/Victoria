using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Victoria {
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions {
        internal static readonly Random Random = new();

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
    }
}