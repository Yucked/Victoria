using System.Net;
using System.Threading.Tasks;
using Victoria.Helpers;
using Victoria.Entities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Victoria
{
    /// <summary>
    /// Handles Lavalink's REST features.
    /// </summary>
    public sealed class LavaRestClient
    {
        private readonly (string Host, int Port, string Password) _rest;

        /// <summary>
        /// Initializes <see cref="LavaRestClient"/>.
        /// </summary>
        /// <param name="host">Lavalink host.</param>
        /// <param name="port">Lavalink port.</param>
        /// <param name="password">Lavalink server password.</param>
        public LavaRestClient(string host, int port, string password)
        {
            _rest.Host = host;
            _rest.Port = port;
            _rest.Password = password;
        }

        /// <summary>
        /// Initializes <see cref="LavaRestClient"/>.
        /// </summary>
        /// <param name="configuration"><see cref="Configuration"/></param>
        public LavaRestClient(Configuration configuration = null)
        {
            configuration ??= new Configuration();
            _rest.Host = configuration.Host;
            _rest.Port = configuration.Port;
            _rest.Password = configuration.Password;
        }

        /// <summary>
        /// Searches Soundcloud for your query.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <returns><see cref="SearchResult"/></returns>
        public Task<SearchResult> SearchSoundcloudAsync(string query)
        {
            return SearchTracksAsync($"scsearch:{query}");
        }

        /// <summary>
        /// Searches YouTube for your query.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <returns><see cref="SearchResult"/></returns>
        public Task<SearchResult> SearchYouTubeAsync(string query)
        {
            return SearchTracksAsync($"ytsearch:{query}");
        }

        /// <summary>
        /// Searches all sources specified in Lavalink's application.yml.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <returns><see cref="SearchResult"/></returns>
        public async Task<SearchResult> SearchTracksAsync(string query, bool loadFullPlaylist = false)
        {
            if (!loadFullPlaylist)
                query = query.SanitizeYoutubeUrl();

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