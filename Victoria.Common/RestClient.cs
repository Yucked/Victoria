using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Victoria.Common
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct RestClient
    {
        private static readonly HttpClient Client;

        static RestClient()
        {
            Client = new HttpClient(new SocketsHttpHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("User-Agent", "Victoria");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<ReadOnlyMemory<byte>> RequestAsync(string url)
        {
            Ensure.NotNull(url);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await Client.SendAsync(request)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                Throw.Exception(response.ReasonPhrase);

            using var content = response.Content;
            var array = await content.ReadAsByteArrayAsync()
                .ConfigureAwait(false);

            Client.DefaultRequestHeaders.Remove("Authorization");
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void WithHeader(string key, string value)
        {
            if (Client.DefaultRequestHeaders.Contains(key))
                return;

            Client.DefaultRequestHeaders.Add(key, value);
        }
    }
}
