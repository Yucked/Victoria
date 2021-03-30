using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Victoria.Resolvers {
    internal sealed class HttpResolver {
        public static HttpResolver Instance
            => _lazyResolver.Value;

        private static readonly Lazy<HttpResolver> _lazyResolver
            = new(() => new());

        private readonly Lazy<HttpClient> _lazyClient
            = new(() => new());

        private HttpClient Client
            => _lazyClient.Value;


        public Task<HttpResponseMessage> GetAsync(string url) {
            return Client.GetAsync(url);
        }
    }
}