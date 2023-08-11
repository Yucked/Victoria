using System;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Victoria.WebSocket.Internal.EventArgs;

namespace Victoria.WebSocket.Internal;

using System.Net.WebSockets;

/// <summary>
/// 
/// </summary>
public sealed class WebSocketClient : IAsyncDisposable {
    /// <summary>
    /// 
    /// </summary>
    public event Func<Task> OnOpenAsync;

    /// <summary>
    /// 
    /// </summary>
    public event Func<CloseEventArgs, Task> OnCloseAsync;

    /// <summary>
    /// 
    /// </summary>
    public event Func<ErrorEventArgs, Task> OnErrorAsync;

    /// <summary>
    /// 
    /// </summary>
    public event Func<DataEventArgs, Task> OnDataAsync;

    /// <summary>
    /// 
    /// </summary>
    public event Func<RetryEventArgs, Task> OnRetryAsync;

    /// <summary>
    /// 
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public Uri Host { get; }

    private readonly Configuration _configuration;
    private readonly ConcurrentQueue<byte[]> _messageQueue;
    private CancellationTokenSource _connectionTokenSource;
    private ClientWebSocket _webSocket;
    private int _reconnectAttempts;
    private int _reconnectDelay;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public WebSocketClient(Configuration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        Host = new Uri($"{configuration.SocketEndpoint}/v{configuration.Version}/websocket");
        _configuration = configuration;
        _webSocket = new ClientWebSocket();
        _messageQueue = new ConcurrentQueue<byte[]>();
        _connectionTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddHeader(string key, string value) {
        if (string.IsNullOrWhiteSpace(key)) {
            throw new ArgumentNullException(nameof(key));
        }

        if (string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentNullException(nameof(value));
        }

        _webSocket.Options.SetRequestHeader(key, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task ConnectAsync() {
        if (_webSocket.State == WebSocketState.Open) {
            throw new InvalidOperationException(
                $"WebSocket is already in open state. Current state: {_webSocket.State}");
        }

        try {
            await _webSocket
                .ConnectAsync(Host, CancellationToken.None)
                .ContinueWith(VerifyConnectionAsync);
        }
        catch (Exception exception) {
            if (exception is not ObjectDisposedException) {
                return;
            }

            ResetWebSocket();
            await ReconnectAsync();
        }

        return;

        async Task VerifyConnectionAsync(Task task) {
            if (task.Exception != null) {
                await OnErrorAsync.Invoke(new ErrorEventArgs(task.Exception));
                await ReconnectAsync();
                return;
            }

            IsConnected = true;
            _reconnectAttempts = 0;
            _connectionTokenSource = new CancellationTokenSource();
            await Task.WhenAll(OnOpenAsync.Invoke(), ReceiveAsync(), SendAsync());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="closeStatus"></param>
    /// <param name="closeReason"></param>
    /// <returns></returns>
    public async Task DisconnectAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure,
                                      string closeReason = "Normal closure.") {
        if (_webSocket.State != WebSocketState.Open) {
            throw new InvalidOperationException(
                $"WebSocket is not in open state. Current state: {_webSocket.State}");
        }

        try {
            await _webSocket.CloseAsync(closeStatus, closeReason, _connectionTokenSource.Token);
        }
        catch (Exception exception) {
            await OnErrorAsync.Invoke(new ErrorEventArgs(exception));
        }
        finally {
            IsConnected = false;
            _connectionTokenSource.Cancel(false);
            await OnCloseAsync.Invoke(new CloseEventArgs());
        }
    }

    private async Task ReceiveAsync() {
        try {
            var buffer = new byte[_configuration.SocketConfiguration.BufferSize];
            var finalBuffer = default(byte[]);
            var offset = 0;
            do {
                var receiveResult = await _webSocket.ReceiveAsync(buffer, _connectionTokenSource.Token);
                if (!receiveResult.EndOfMessage) {
                    finalBuffer = new byte[_configuration.SocketConfiguration.BufferSize * 2];
                    buffer.CopyTo(finalBuffer, offset);
                    offset += receiveResult.Count;
                    buffer = new byte[_configuration.SocketConfiguration.BufferSize];
                    continue;
                }

                switch (receiveResult.MessageType) {
                    case WebSocketMessageType.Text:
                        var array = finalBuffer ?? buffer;
                        Array.Resize(ref array, Array.FindLastIndex(array, b => b != 0) + 1);
                        await OnDataAsync.Invoke(new DataEventArgs(array));

                        finalBuffer = default;
                        buffer = new byte[_configuration.SocketConfiguration.BufferSize];
                        offset = 0;
                        break;

                    case WebSocketMessageType.Close:
                        await DisconnectAsync();
                        await ReconnectAsync();
                        break;

                    case WebSocketMessageType.Binary:
                    default:
                        break;
                }
            } while (_webSocket.State == WebSocketState.Open &&
                     !_connectionTokenSource.IsCancellationRequested);
        }
        catch (Exception exception) {
            if (exception is TaskCanceledException or OperationCanceledException or ObjectDisposedException) {
                return;
            }

            await OnErrorAsync.Invoke(new ErrorEventArgs(exception));
        }
    }

    private async Task SendAsync() {
        try {
            do {
                if (!_messageQueue.TryDequeue(out var content)) {
                    await Task.Delay(500);
                    continue;
                }

                await _webSocket.SendAsync(content, WebSocketMessageType.Text,
                    true, _connectionTokenSource.Token);
            } while (_webSocket.State == WebSocketState.Open &&
                     !_connectionTokenSource.IsCancellationRequested);
        }
        catch (Exception exception) {
            await OnErrorAsync.Invoke(new ErrorEventArgs(exception));
        }
    }

    private async Task ReconnectAsync() {
        if (_configuration.SocketConfiguration.ReconnectAttempts <= 0 ||
            _configuration.SocketConfiguration.ReconnectAttempts <= _reconnectAttempts) {
            await OnRetryAsync.Invoke(new RetryEventArgs(0, true));
            return;
        }

        _connectionTokenSource.Cancel(false);
        _reconnectDelay += _configuration.SocketConfiguration.ReconnectDelay;
        _reconnectAttempts++;

        await OnRetryAsync.Invoke(new RetryEventArgs(_reconnectAttempts, false));
        await Task.Delay(_reconnectDelay);
        await ConnectAsync();
    }

    private void ResetWebSocket() {
        var options = _webSocket.Options;
        var headerCollection = options.GetType()
                .GetProperty("RequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(options, null)
            as WebHeaderCollection;

        _webSocket = new ClientWebSocket();
        foreach (var key in headerCollection.Keys) {
            _webSocket.Options.SetRequestHeader($"{key}", headerCollection.Get($"{key}"));
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (IsConnected) {
            await DisconnectAsync();
        }

        _connectionTokenSource?.Dispose();
        _webSocket.Dispose();
        _messageQueue.Clear();
    }
}