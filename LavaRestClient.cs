using System.Net;
using System.Threading.Tasks;
using Victoria.Helpers;
using Victoria.Entities;
using Newtonsoft.Json.Linq;
using Victoria.Entities;
using System.Collections.Generic;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LavaRestClient
    {
        private readonly (string Host, int Port, string Password) _rest;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="password"></param>
        public LavaRestClient(string host, int port, string password)
        {
            _rest.Host = host;
            _rest.Port = port;
            _rest.Password = password;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public LavaRestClient(Configuration configuration = null)
        {
            configuration ??= new Configuration();
            _rest.Host = configuration.Host;
            _rest.Port = configuration.Port;
            _rest.Password = configuration.Password;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<SearchResult> SearchSoundcloudAsync(string query)
        {
            return TracksRequestAsync($"scsearch:{query}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<SearchResult> SearchYoutubeAsync(string query)
        {
            return TracksRequestAsync($"ytsearch:{query}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<SearchResult> TracksRequestAsync(string query)
        {
            var url = $"http://{_rest.Host}:{_rest.Port}/loadtracks?identifier={WebUtility.UrlEncode(query)}";
            var request = await HttpHelper.Instance
                .WithCustomHeader("Authorization", _rest.Password)
                .GetStringAsync(url).ConfigureAwait(false);

            var json = JObject.Parse(request);
            var result = json.ToObject<SearchResult>();
            var trackInfo = json.GetValue("tracks").ToObject<JArray>();
            var hashset = new HashSet<LavaTrack>();

            foreach (var info in trackInfo)
            {
                var track = info["info"].ToObject<LavaTrack>();
                track.Hash = info["track"].ToObject<string>();
                hashset.Add(track);
            }

            result.Tracks = hashset;
            return result;
        }
    }
}