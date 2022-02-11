using System;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Victoria {
    /// <summary>
    /// Wrapper around <see cref="ClientWebSocket"/> to make ws connections easier to handle.
    /// </summary>
    public sealed class LavaSocket : IAsyncDisposable {
        /// <summary>
        /// Fires when connection is established.
        /// </summary>
        public event Func<Task> OnOpenAsync;

        /// <summary>
        /// Fires when either client or server closes connection.
        /// </summary>
        public event Func<string, Task> OnCloseAsync;

        /// <summary>
        /// Fires when data is received from server.
        /// </summary>
        public event Func<byte[], Task> OnDataAsync;

        /// <summary>
        /// Fires when a non-generic error is received.
        /// </summary>
        public event Func<Exception, Task> OnErrorAsync;

        /// <summary>
        /// Fires when connection is lost and a retry attempt is made.
        /// </summary>
        public event Func<int, TimeSpan, bool, Task> OnRetryAsync;

        private readonly LavaConfig _lavaConfig;
        private readonly Uri _url;
        private int _connectionAttempts;
        private TimeSpan _reconnectInterval;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationToken;
        private bool _isConnected;

        internal LavaSocket(LavaConfig lavaConfig) {
            _lavaConfig = lavaConfig;
            _webSocket = new ClientWebSocket();
            _url = new Uri($"{(_lavaConfig.IsSsl ? "wss" : "ws")}://{_lavaConfig.Hostname}:{_lavaConfig.Port}");
        }

        /// <summary>
        /// Set websocket headers before making a handshake request.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
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
        /// Initializes instance of <see cref="ClientWebSocket"/> and connects to server.
        /// </summary>
        public async Task ConnectAsync() {
            if (_webSocket.State == WebSocketState.Open) {
                throw new InvalidOperationException(
                    $"WebSocket is already in open state. Current state: {_webSocket.State}");
            }

            async Task VerifyConnectionAsync(Task task) {
                if (task.Exception != null) {
                    await OnErrorAsync.Invoke(task.Exception);
                    await RetryConnectionAsync();
                    return;
                }

                _isConnected = true;
                _connectionAttempts = 0;
                _reconnectInterval = TimeSpan.Zero;
                _cancellationToken = new CancellationTokenSource();
                await OnOpenAsync.Invoke();
                await ReceiveAsync();
            }

            try {
                await _webSocket
                    .ConnectAsync(_url, CancellationToken.None)
                    .ContinueWith(VerifyConnectionAsync);
            }
            catch (Exception exception) {
                if (exception is not ObjectDisposedException) {
                    return;
                }

                await RetryConnectionAsync();
            }
        }

        /// <summary>
        /// Used to send data to websocket server.
        /// </summary>
        /// <param name="value">Value of <typeparamref name="T"/></param>
        /// <typeparam name="T">Type of data to set.</typeparam>
        /// <exception cref="InvalidOperationException">Throws if not connected to websocket.</exception>
        public async Task SendAsync<T>(T value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value), "Provided data was null.");
            }

            if (_webSocket.State != WebSocketState.Open) {
                throw new InvalidOperationException(
                    $"WebSocket is not in open state. Current state: {_webSocket.State}");
            }

            try {
                var rawBytes = JsonSerializer.SerializeToUtf8Bytes(value, VictoriaExtensions.JsonOptions);
                await _webSocket.SendAsync(rawBytes, WebSocketMessageType.Text, true, _cancellationToken.Token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception) {
                await OnErrorAsync.Invoke(exception);
            }
        }

        /// <summary>
        /// Closes connection to websocket server.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure,
                                          string closeReason = "Normal closure.") {
            if (_webSocket.State != WebSocketState.Open) {
                throw new InvalidOperationException(
                    $"WebSocket is not in open state. Current state: {_webSocket.State}");
            }

            try {
                await _webSocket.CloseAsync(closeStatus, closeReason, _cancellationToken.Token);
            }
            catch (Exception exception) {
                await OnErrorAsync.Invoke(exception);
            }
            finally {
                _isConnected = false;
                _cancellationToken.Cancel(false);
                await OnCloseAsync.Invoke(closeReason);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            if (_isConnected) {
                await DisconnectAsync();
            }

            _cancellationToken?.Dispose();
            _webSocket.Dispose();
        }

        private async Task RetryConnectionAsync() {
            _cancellationToken?.Cancel(false);
            if (_lavaConfig.ReconnectAttempts <= 0 ||
                _lavaConfig.ReconnectAttempts <= _connectionAttempts) {
                await OnRetryAsync(0, _reconnectInterval, true);
                return;
            }

            _reconnectInterval += _lavaConfig.ReconnectDelay;
            _connectionAttempts++;
            await OnRetryAsync.Invoke(_connectionAttempts, _reconnectInterval, false);
            ResetWebSocket();
            await Task.Delay(_reconnectInterval)
                .ContinueWith(_ => ConnectAsync())
                .ConfigureAwait(false);
        }

        private async Task ReceiveAsync() {
            try {
                var buffer = new byte[_lavaConfig.BufferSize];
                var finalBuffer = default(byte[]);

                var offset = 0;
                do {
                    var receiveResult = await _webSocket.ReceiveAsync(buffer, _cancellationToken.Token);
                    if (!receiveResult.EndOfMessage) {
                        finalBuffer = new byte[_lavaConfig.BufferSize * 2];
                        buffer.CopyTo(finalBuffer, offset);
                        offset += receiveResult.Count;
                        buffer = new byte[_lavaConfig.BufferSize];
                        continue;
                    }

                    switch (receiveResult.MessageType) {
                        case WebSocketMessageType.Text:
                            await OnDataAsync.Invoke(RemoveTrailingNulls(finalBuffer ?? buffer));
                            finalBuffer = default;
                            buffer = new byte[_lavaConfig.BufferSize];
                            offset = 0;
                            break;

                        case WebSocketMessageType.Close:
                            await DisconnectAsync();
                            break;
                    }
                } while (_webSocket.State == WebSocketState.Open &&
                         !_cancellationToken.IsCancellationRequested);
            }
            catch (Exception exception) {
                if (exception is TaskCanceledException ||
                    exception is OperationCanceledException ||
                    exception is ObjectDisposedException) {
                    return;
                }

                await OnErrorAsync.Invoke(exception);
                await RetryConnectionAsync();
            }
        }

        internal static byte[] RemoveTrailingNulls(byte[] array) {
            Array.Resize(ref array, Array.FindLastIndex(array, b => b != 0) + 1);
            return array;
        }

        private void ResetWebSocket() {
            var options = _webSocket.Options;
            var headerCollection = options.GetType()
                    .GetProperty("RequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(options, null)
                as WebHeaderCollection;

            _webSocket.Dispose();
            _webSocket = new ClientWebSocket();
            _cancellationToken = new CancellationTokenSource();
            foreach (var key in headerCollection.Keys) {
                _webSocket.Options.SetRequestHeader($"{key}", headerCollection.Get($"{key}"));
            }
        }
    }
}