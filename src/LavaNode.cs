using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;
using Victoria.Rest;
using Victoria.WebSocket;

namespace Victoria;

/// <inheritdoc />
public class LavaNode<TLavaPlayer, TLavaTrack> : IAsyncDisposable
    where TLavaTrack : ILavaTrack
    where TLavaPlayer : ILavaPlayer<TLavaTrack> {
    private LavaSocket<TLavaPlayer, TLavaTrack> Socket { get; }
    private LavaRest<TLavaPlayer, TLavaTrack> Rest { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseSocketClient"></param>
    /// <param name="configuration"></param>
    /// <param name="loggerFactory"></param>
    public LavaNode(BaseSocketClient baseSocketClient,
                    Configuration configuration,
                    ILoggerFactory loggerFactory) {
        var httpClient = new HttpClient();
        Socket = new LavaSocket<TLavaPlayer, TLavaTrack>(baseSocketClient,
            configuration,
            loggerFactory.CreateLogger<LavaSocket<TLavaPlayer, TLavaTrack>>());
        Rest = new LavaRest<TLavaPlayer, TLavaTrack>(httpClient,
            configuration,
            loggerFactory.CreateLogger<LavaRest<TLavaPlayer, TLavaTrack>>());
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        await Socket.DisconnectAsync();
        await Socket.DisposeAsync();
        await Rest.DisposeAsync();
    }
}