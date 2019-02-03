using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Victoria.Entities.Payloads;

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
        }

        public void SetHeaders(IReadOnlyDictionary<string, string> headers = null)
        {
            CheckClient();
            _client.DefaultRequestHeaders.Clear();
            foreach (var header in headers)
                _client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        public void ClearHeaders()
        {
            CheckClient();
            _client.DefaultRequestHeaders.Clear();
        }

        public async ValueTask<string> GetStringAsync(string url)
        {
            CheckClient();

            var get = await _client.GetAsync(url);
            if (!get.IsSuccessStatusCode)
                return string.Empty;

            using (var content = get.Content)
            {
                return await content.ReadAsStringAsync();
            }
        }

        public async Task SendPayloadAsync(BasePayload payload)
        {
        }
    }
}