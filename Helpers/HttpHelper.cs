using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Victoria.Helpers
{
    internal sealed class HttpHelper
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

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("User-Agent", "Victoria");
        }

        public async ValueTask<string> GetStringAsync(string url)
        {
            CheckClient();

            var get = await _client.GetAsync(url).ConfigureAwait(false);
            if (!get.IsSuccessStatusCode)
                return string.Empty;

            using (var content = get.Content)
            {
                return await content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public async ValueTask<Stream> HeadersRequestAsync(string url, params (string name, string value)[] headers)
        {
            CheckClient();

            foreach (var (name, value) in headers)
                _client.DefaultRequestHeaders.Add(name, value);

            var get = await _client.GetAsync(url).ConfigureAwait(false);

            if (!get.IsSuccessStatusCode)
                return default;

            using (var content = get.Content)
            {
                return await content.ReadAsStreamAsync().ConfigureAwait(false);
            }
        }
    }
}