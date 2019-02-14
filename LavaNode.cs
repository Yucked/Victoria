using Discord;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Victoria.Entities.Responses.LoadTracks;
using Victoria.Helpers;

namespace Victoria
{
    public sealed class LavaNode : IAsyncDisposable
    {
        public event Func<LogMessage, ValueTask> Log;


        private readonly LavaNodeSettings _nodeSettings;
        private readonly ConcurrentDictionary<ulong, LavaPlayer> _players;

        internal LavaNode(LavaNodeSettings nodeSettings = default)
        {
            _players = new ConcurrentDictionary<ulong, LavaPlayer>();
        }

        private async ValueTask<LoadResult> TracksRequestAsync(string query)
        {
            var url = $"http://{_nodeSettings.Host}:{_nodeSettings.Port}/loadtracks?identifier={WebUtility.UrlEncode(query)}";
            using var request = await HttpHelper.Instance.HeadersRequestAsync(url, ("Authorization", _nodeSettings.Authorization));
            using var reader = new StreamReader(request, Encoding.UTF8);
            var loadResult = JsonConverter.FromJson<LoadResult>(await reader.ReadToEndAsync());
            return loadResult;
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