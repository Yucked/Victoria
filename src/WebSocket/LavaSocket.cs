using System;
using System.Text.Json;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;
using Victoria.WebSocket.EventArgs;
using Victoria.WebSocket.Internal;
using Victoria.WebSocket.Internal.EventArgs;

namespace Victoria.WebSocket;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TLavaPlayer"></typeparam>
/// <typeparam name="TLavaTrack"></typeparam>
public class LavaSocket<TLavaPlayer, TLavaTrack> : IAsyncDisposable
    where TLavaTrack : ILavaTrack
    where TLavaPlayer : ILavaPlayer<TLavaTrack> {
    /// <summary>
    /// 
    /// </summary>
    public event Func<ReadyEventArg, Task> OnReady;

    /// <summary>
    /// 
    /// </summary>
    public event Func<StatsEventArg, Task> OnStats;

    /// <summary>
    /// 
    /// </summary>
    public event Func<PlayerUpdateEventArg<TLavaPlayer, TLavaTrack>, Task> OnPlayerUpdate;

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

    /// <summary>
    /// 
    /// </summary>
    public string SessionId { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsConnected { get; internal set; }

    private readonly Configuration _configuration;
    private readonly ILogger<LavaSocket<TLavaPlayer, TLavaTrack>> _logger;
    private readonly WebSocketClient _webSocketClient;
    private readonly BaseSocketClient _baseSocketClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="socketClient"></param>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
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
    ///     Starts a WebSocket connection to the specified <see cref="Configuration.Hostname" />:<see cref="Configuration.Port" />
    ///     and hooks into <see cref="BaseSocketClient" /> events.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if client is already connected.</exception>
    public async Task ConnectAsync() {
        if (IsConnected) {
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
        if (!IsConnected) {
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

    private Task OnOpenAsync() {
        IsConnected = true;

        // TODO: Handle resume?
        return Task.CompletedTask;
    }

    private Task OnCloseAsync(CloseEventArgs arg) {
        IsConnected = false;
        _logger.LogWarning("WebSocket connection closed");
        return Task.CompletedTask;
    }

    private Task OnErrorAsync(ErrorEventArgs arg) {
        _logger.LogError("{exception}, {message}", arg.Exception, arg.Message);
        return Task.CompletedTask;
    }

    private Task OnRetryAsync(RetryEventArgs arg) {
        if (arg.IsLastRetry) {
            _logger.LogError("This was the last try in establishing connection with Lavalink");
            return Task.CompletedTask;
        }

        _logger.LogWarning("Lavalink reconnect attempt #{attempts}", arg.Count);
        return Task.CompletedTask;
    }

    private async Task OnDataAsync(DataEventArgs arg) {
        if (arg.Data.Length == 0) {
            _logger.LogWarning("Didn't receive any data from websocket");
            return;
        }

        _logger.LogDebug("{data}", JsonSerializer.Serialize(arg.Data));
        var document = JsonDocument.Parse(arg.Data).RootElement;

        try {
            switch (document.TryGetProperty("op", out var element) ? default : $"{element}") {
                case "ready":
                    if (OnReady == null) {
                        return;
                    }

                    SessionId = $"{document.GetProperty("sessionId")}";
                    await OnReady.Invoke(new ReadyEventArg(
                        document.GetProperty("resumed").GetBoolean(),
                        SessionId));
                    break;

                case "stats":
                    if (OnStats == null) {
                        return;
                    }

                    await OnStats.Invoke(JsonSerializer.Deserialize<StatsEventArg>(arg.Data));
                    break;

                case "playerUpdate": {
                    break;
                }

                case "event": {
                    break;
                }

                default: {
                    _logger.LogCritical("Unknown OP code encountered, please check lavalink implementation.");
                    break;
                }
            }
        }
        catch (Exception exception) {
            _logger.LogError("{message} {exception}", exception.Message, exception);
        }
    }
}