using System;
using System.Text.Json;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Victoria.Enums;
using Victoria.Interfaces;
using Victoria.WebSocket.EventArgs;
using Victoria.WebSocket.Internal;
using Victoria.WebSocket.Internal.EventArgs;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
    public event Func<PlayerUpdateEventArg, Task> OnPlayerUpdate;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackStartEventArg, Task> OnTrackStart;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackEndEventArg, Task> OnTrackEnd;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackExceptionEventArg, Task> OnTrackException;

    /// <summary>
    /// 
    /// </summary>
    public event Func<TrackStuckEventArg, Task> OnTrackStuck;

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

        try {
            var document = JsonDocument.Parse(arg.Data).RootElement;
            var guildId = document.TryGetProperty("guildId", out var idElement)
                ? ulong.Parse(idElement.GetString()!)
                : 0;
            switch (document.TryGetProperty("op", out var element) ? default : $"{element}") {
                case "ready":
                    if (OnReady == null) {
                        _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                            nameof(OnReady));
                        return;
                    }

                    SessionId = $"{document.GetProperty("sessionId")}";
                    await OnReady.Invoke(new ReadyEventArg(
                        document.GetProperty("resumed").GetBoolean(),
                        SessionId));
                    break;

                case "stats":
                    if (OnStats == null) {
                        _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                            nameof(OnStats));
                        return;
                    }

                    await OnStats.Invoke(JsonSerializer.Deserialize<StatsEventArg>(arg.Data));
                    break;

                case "playerUpdate":
                    var state = document.GetProperty("state");
                    if (OnPlayerUpdate == null) {
                        _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                            nameof(OnPlayerUpdate));
                        return;
                    }

                    await OnPlayerUpdate.Invoke(new PlayerUpdateEventArg {
                        GuildId = guildId,
                        Time = DateTimeOffset.FromUnixTimeMilliseconds(state.GetProperty("time").GetInt32()),
                        Position = TimeSpan.FromMilliseconds(state.GetProperty("position").GetInt32()),
                        IsConnected = state.GetProperty("connected").GetBoolean(),
                        Ping = state.GetProperty("ping").GetInt64()
                    });
                    break;

                case "event":
                    switch (document.GetProperty("type").GetString()) {
                        case "TrackStartEvent":
                            if (OnTrackStart == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackStart));
                                return;
                            }

                            await OnTrackStart.Invoke(new TrackStartEventArg {
                                GuildId = guildId,
                                EncodedTrack = document.GetProperty("encodedTrack").GetString()
                            });
                            break;

                        case "TrackEndEvent":
                            if (OnTrackEnd == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackEnd));
                                return;
                            }

                            await OnTrackEnd.Invoke(new TrackEndEventArg {
                                GuildId = guildId,
                                EncodedTrack = document.GetProperty("encodedTrack").GetString(),
                                Reason = document.GetProperty("reason").Deserialize<TrackEndReason>()
                            });
                            break;

                        case "TrackExceptionEvent":
                            if (OnTrackException == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackException));
                                return;
                            }

                            await OnTrackException.Invoke(new TrackExceptionEventArg {
                                GuildId = guildId,
                                EncodedTrack = document.GetProperty("encodedTrack").GetString(),
                                Exception = document.GetProperty("exception").Deserialize<TrackException>()
                            });
                            break;

                        case "TrackStuckEvent":
                            if (OnTrackStuck == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnTrackStuck));
                                return;
                            }

                            await OnTrackStuck.Invoke(new TrackStuckEventArg {
                                GuildId = guildId,
                                EncodedTrack = document.GetProperty("EncodedTrack").GetString(),
                                Threshold = document.GetProperty("thresholdMs").GetInt64()
                            });
                            break;

                        case "WebSocketClosedEvent":
                            if (OnWebSocketClosed == null) {
                                _logger.LogDebug("Not firing {eventName} since it isn't subscribed.",
                                    nameof(OnWebSocketClosed));
                                return;
                            }

                            await OnWebSocketClosed.Invoke(new WebSocketClosedEventArg {
                                GuildId = guildId,
                                ByRemote = document.GetProperty("byRemote").GetBoolean(),
                                Code = document.GetProperty("code").GetInt32(),
                                Reason = document.GetProperty("reason").GetString()
                            });
                            break;

                        default:
                            _logger.LogError("Unknown event encountered {}. Please open an issue.",
                                document.GetProperty("type"));
                            break;
                    }

                    break;

                default: {
                    _logger.LogCritical("Unknown OP code encountered, please check lavalink implementation.");
                    break;
                }
            }
        }
        catch (Exception exception) {
            _logger.LogError(exception is JsonReaderException
                ? "Please increase buffer size in configuration."
                : $"{exception.Message} {exception}");
        }
    }
}