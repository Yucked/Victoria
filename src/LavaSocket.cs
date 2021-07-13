using System;
using System.Collections.Generic;
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
        /// Fires when connection is lost and a retry attempt is made.
        /// </summary>
        public event Func<string, Task> OnRetry;

        /// <summary>
        /// Fires when connection is established.
        /// </summary>
        public event Func<Task> OnConnected;

        /// <summary>
        /// Fires when either client or server closes connection.
        /// </summary>
        public event Func<string, Task> OnDisconnected;

        /// <summary>
        /// Fires when data is received from server.
        /// </summary>
        public event Func<byte[], Task> OnReceive;

        private readonly LavaConfig _lavaConfig;
        private readonly Uri _url;
        private int _connectionAttempts;
        private TimeSpan _reconnectInterval;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationToken;
        private bool _isConnected;

        internal LavaSocket(LavaConfig lavaConfig) {
            _lavaConfig = lavaConfig;
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
                    await ReconnectAsync();
                    return;
                }

                _isConnected = true;
                _cancellationToken = new CancellationTokenSource();
                await Task.WhenAll(OnOpenAsync.Invoke(), ReceiveAsync(), SendAsync());
            }

            if (_connectionAttempts == _lavaConfig.ReconnectAttempts) {
                return;
            }

            try {
                await _webSocket.ConnectAsync(_url, CancellationToken.None).ContinueWith(VerifyConnectionAsync);
            }
            catch {
                // IGNORE
            }
        }

        /// <summary>
        /// Used to send data to websocket server.
        /// </summary>
        /// <param name="value">Value of <typeparamref name="T"/></param>
        /// <typeparam name="T">Type of data to set.</typeparam>
        /// <exception cref="InvalidOperationException">Throws if not connected to websocket.</exception>
        public async Task SendAsync<T>(T value) {
            if (_webSocket.State != WebSocketState.Open) {
                throw new InvalidOperationException("WebSocket connection to server isn't in open state.");
            }

            var rawBytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await _webSocket.SendAsync(rawBytes, WebSocketMessageType.Text, true, _cancellationToken.Token)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Closes connection to websocket server.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync() {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Requested.", _cancellationToken.Token)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            if (_webSocket.State == WebSocketState.Open) {
                await _webSocket.CloseAsync(WebSocketCloseStatus.Empty, "", _cancellationToken.Token)
                    .ConfigureAwait(false);
            }

            _cancellationToken.CancelAfter(TimeSpan.FromSeconds(5));
            _webSocket.Dispose();
        }

        private async Task VerifyConnectionAsync(Task task) {
            if (task.IsCanceled || task.IsFaulted || task.Exception != null) {
                _isConnected = false;
                await RetryConnectionAsync();
            }
            else {
                _isConnected = true;
                _connectionAttempts = 0;
                OnConnected?.Invoke();
                await ReceiveAsync()
                    .ConfigureAwait(false);
            }
        }

        private async Task RetryConnectionAsync() {
            _cancellationToken.Cancel(false);

            if (_connectionAttempts > _lavaConfig.ReconnectAttempts && _lavaConfig.ReconnectAttempts != -1) {
                return;
            }

            Interlocked.Increment(ref _connectionAttempts);
            _reconnectInterval += _lavaConfig.ReconnectDelay;
            OnRetry?.Invoke(_connectionAttempts == _lavaConfig.ReconnectAttempts
                ? "This was the last attempt at re-establishing websocket connection."
                : $"Waiting {_reconnectInterval.TotalSeconds}s before attempt #{_connectionAttempts}.");

            await Task.Delay(_reconnectInterval)
                .ContinueWith(_ => ConnectAsync())
                .ConfigureAwait(false);
        }

        private async Task ReceiveAsync() {
            try {
                while (_webSocket.State == WebSocketState.Open && !_cancellationToken.IsCancellationRequested) {
                    var buffer = new byte[_lavaConfig.BufferSize];
                    var result = await _webSocket.ReceiveAsync(buffer, _cancellationToken.Token)
                        .ConfigureAwait(false);

                    switch (result.MessageType) {
                        case WebSocketMessageType.Close:
                            _isConnected = false;
                            OnDisconnected?.Invoke("Server closed the connection!");

                            await RetryConnectionAsync()
                                .ConfigureAwait(false);
                            break;

                        case WebSocketMessageType.Text:
                            if (!result.EndOfMessage) {
                                continue;
                            }

                            var startLength = buffer.Length - 1;
                            while (buffer[startLength] == 0) {
                                --startLength;
                            }

                            var cleaned = new byte[startLength + 1];
                            Array.Copy(buffer, cleaned, startLength + 1);
                            OnReceive?.Invoke(cleaned);
                            break;
                    }
                }
            }
            catch (Exception ex) {
                OnDisconnected?.Invoke(ex.Message);
                await ConnectAsync()
                    .ConfigureAwait(false);
            }
        }

        private void ResetWebSocket() {
            var options = _webSocket.Options;
            var headerCollection = options.GetType()
                    .GetProperty("RequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(options, null)
                as WebHeaderCollection;

            _webSocket = new ClientWebSocket();
            _cancellationToken = new CancellationTokenSource();
            foreach (var key in headerCollection.Keys) {
                _webSocket.Options.SetRequestHeader($"{key}", headerCollection.Get($"{key}"));
            }
        }
    }
}