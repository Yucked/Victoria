using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Victoria.Configs;
using Victoria.Helpers;
using Victoria.Entities;

namespace Victoria
{
    public sealed class LavaRest
    {
        private readonly EndpointConfig _config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeSettings"></param>
        public LavaRest(EndpointConfig config = null)
        {
            _config = config is null ? new EndpointConfig() : config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ValueTask<SearchResult> SearchSoundcloudAsync(string query)
        {
            return TracksRequestAsync($"scsearch:{query}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ValueTask<SearchResult> SearchYoutubeAsync(string query)
        {
            return TracksRequestAsync($"ytsearch:{query}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async ValueTask<SearchResult> TracksRequestAsync(string query)
        {
            var url = $"http://{_config.Host}:{_config.Port}/loadtracks?identifier={WebUtility.UrlEncode(query)}";
            var request = await HttpHelper.Instance
                .WithCustomHeaders(("Authorization", _config.Authorization))
                .GetStringAsync(url).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<SearchResult>(request);
        }
    }
}