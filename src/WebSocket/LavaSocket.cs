using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;
using Victoria.WebSocket.EventArgs;
using Victoria.WebSocket.Internal;

namespace Victoria.WebSocket;

public class LavaSocket<TLavaPlayer, TLavaTrack> : IAsyncDisposable
    where TLavaTrack : ILavaTrack
    where TLavaPlayer : ILavaPlayer<TLavaTrack> {
    /// <summary>
    /// 
    /// </summary>
    public event Func<StatsEventArg, Task> OnStatsReceived;

    /// <summary>
    /// 
    /// </summary>
    public event Func<PlayerUpdateEventArg<TLavaPlayer, TLavaTrack>, Task> OnUpdateReceived;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackStartEventArg<TLavaPlayer, TLavaTrack>, Task> OnTrackStart;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackEndEventArg<TLavaPlayer, TLavaTrack>, Task> OnTrackEnd;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackExceptionEventArg<TLavaPlayer, TLavaTrack>, Task> OnTrackException;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackStuckEventArg<TLavaPlayer, TLavaTrack>, Task> OnTrackStuck;

    /// <summary>
    /// 
    /// </summary>
    public event Func<WebSocketClosedEventArg, Task> OnWebSocketClosed;

    private readonly Configuration _configuration;
    private readonly ILogger<LavaSocket<TLavaPlayer, TLavaTrack>> _logger;
    private readonly WebSocketClient _webSocketClient;
    private readonly BaseSocketClient _baseSocketClient;

    private bool _refConnected;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <param name="webSocketClient"></param>
    public LavaSocket(BaseSocketClient socketClient,
                      Configuration configuration,
                      ILogger<LavaSocket<TLavaPlayer, TLavaTrack>> logger) {
        _configuration = configuration;
        _logger = logger;
        _baseSocketClient = socketClient;

        _webSocketClient = new WebSocketClient(_configuration);
        _webSocketClient.OnOpenAsync += OnOpenAsync;
        _webSocketClient.OnErrorAsync += OnErrorAsync;
        _webSocketClient.OnCloseAsync += OnCloseAsync;
        _webSocketClient.OnDataAsync += OnDataAsync;
        _webSocketClient.OnRetryAsync += OnRetryAsync;
    }

    /// <summary>
    ///     Starts a WebSocket connection to the specified <see cref="NodeConfiguration.Hostname" />:<see cref="NodeConfiguration.Port" />
    ///     and hooks into <see cref="BaseSocketClient" /> events.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if client is already connected.</exception>
    public async Task ConnectAsync() {
        if (Volatile.Read(ref _refConnected)) {
            throw new InvalidOperationException(
                $"You must call {nameof(DisconnectAsync)} or {nameof(DisposeAsync)} before calling {nameof(ConnectAsync)}.");
        }

        if (_baseSocketClient.CurrentUser == null || _baseSocketClient.CurrentUser.Id == 0) {
            throw new InvalidOperationException($"{nameof(_baseSocketClient)} is not in ready state.");
        }

        _webSocketClient.AddHeader("Authorization", _configuration.Authorization);
        _webSocketClient.AddHeader("User-Id", $"{_baseSocketClient.CurrentUser.Id}");
        _webSocketClient.AddHeader("Client-Name",
            $"{nameof(Victoria)}/{typeof(Configuration).Assembly.GetName().Version}");

        await _webSocketClient.ConnectAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Disposes all players and closes websocket connection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if client isn't connected.</exception>
    public async Task DisconnectAsync() {
        if (!Volatile.Read(ref _refConnected)) {
            throw new InvalidOperationException("Can't disconnect when client isn't connected.");
        }

        await _webSocketClient.DisconnectAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        await _webSocketClient.DisposeAsync()
            .ConfigureAwait(false);
    }
}