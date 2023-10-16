using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Victoria {
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions {
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

        internal static T AsEnum<T>(this JsonElement element) where T : struct {
            return Enum.Parse<T>(element.GetString()!, true);
        }
    }
}