using Discord;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Victoria.Entities.Responses.LoadTracks;
using Victoria.Helpers;

namespace Victoria
{
    public sealed class LavaNode : IAsyncDisposable
    {
        private readonly LavaNodeSettings _nodeSettings;
        private readonly ConcurrentDictionary<ulong, LavaPlayer> _players;

        internal LavaNode(LavaNodeSettings nodeSettings)
        {
            _nodeSettings = nodeSettings;
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();
        }

        internal Task InitializeAsync()
            => Task.CompletedTask;

        public ValueTask<LoadResult> SearchSoundcloudAsync(string query)
        {
            return TracksRequestAsync($"scsearch:{query}");
        }

        public ValueTask<LoadResult> SearchYoutubeAsync(string query)
        {
            return TracksRequestAsync($"ytsearch:{query}");
        }

        private async ValueTask<LoadResult> TracksRequestAsync(string query)
        {
            var url = $"http://{_nodeSettings.Host}:{_nodeSettings.Port}/loadtracks?identifier={WebUtility.UrlEncode(query)}";
            var request = await HttpHelper.Instance.HeadersRequestAsync(url, ("Authorization", _nodeSettings.Authorization));
            return JsonSerializer.Parse<LoadResult>(request);
        }

        public async ValueTask DisposeAsync()
        {
            foreach(var player in _players)
            {
                await player.Value.DisposeAsync();
            }

            _players.Clear();
        }
    }
}