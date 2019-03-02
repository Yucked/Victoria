using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Victoria.Helpers
{
    public sealed class HttpHelper
    {
        private static readonly Lazy<HttpHelper> LazyHelper
            = new Lazy<HttpHelper>(() => new HttpHelper());

        private HttpClient _client;

        public static HttpHelper Instance
            => LazyHelper.Value;

        private void CheckClient()
        {
            if (!(_client is null))
                return;

            _client = new HttpClient(new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Victoria");
        }

        public async Task<string> GetStringAsync(string url)
        {
            CheckClient();

            var get = await _client.GetAsync(url).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
                return string.Empty;

            using var content = get.Content;
            var read = await content.ReadAsStringAsync().ConfigureAwait(false);
            return read;
        }

        public HttpHelper WithCustomHeader(string key, string value)
        {
            CheckClient();

            if (_client.DefaultRequestHeaders.Contains(key))
                return this;

            _client.DefaultRequestHeaders.Add(key, value);
            return this;
        }
    }
}