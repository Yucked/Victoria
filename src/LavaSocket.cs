using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Victoria {
    /// <summary>
    /// </summary>
    public sealed class LavaSocket : IAsyncDisposable {
        /// <summary>
        /// 
        /// </summary>
        public event Func<string, Task> OnRetry;

        /// <summary>
        /// 
        /// </summary>
        public event Func<Task> OnConnected;

        /// <summary>
        /// 
        /// </summary>
        public event Func<string, Task> OnDisconnected;

        /// <summary>
        /// 
        /// </summary>
        public event Func<byte[], Task> OnReceive;

        private readonly LavaConfig _lavaConfig;
        private int _connectionAttempts;
        private TimeSpan _reconnectInterval;
        private ClientWebSocket _socket;
        private CancellationTokenSource _tokenSource;
        private bool _refIsUsable;
        private readonly IDictionary<string, string> _headers;

        internal LavaSocket(LavaConfig lavaConfig) {
            _lavaConfig = lavaConfig;
            _headers = new Dictionary<string, string>(3);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            if (_socket.State == WebSocketState.Open)
                await _socket.CloseAsync(WebSocketCloseStatus.Empty, "", _tokenSource.Token)
                    .ConfigureAwait(false);

            _tokenSource.CancelAfter(TimeSpan.FromSeconds(5));
            _socket.Dispose();
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetHeader(string key, string value) {
            if (_headers.ContainsKey(key))
                return;

            _headers.Add(key, value);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync() {
            _tokenSource = new CancellationTokenSource();

            _socket = new ClientWebSocket();
            foreach (var (key, value) in _headers)
                _socket.Options.SetRequestHeader(key, value);

            var url = new Uri($"ws://{_lavaConfig.Hostname}:{_lavaConfig.Port}");

            if (_connectionAttempts == _lavaConfig.ReconnectAttempts)
                return;

            try {
                await _socket.ConnectAsync(url, CancellationToken.None).ContinueWith(VerifyConnectionAsync);
            }
            catch {
                // IGNORE
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendAsync<T>(T value) {
            if (_socket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket state is invalid.");

            var rawBytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await _socket.SendAsync(rawBytes, WebSocketMessageType.Text, true, _tokenSource.Token)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync() {
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Requested.", _tokenSource.Token)
                .ConfigureAwait(false);
        }

        private async Task VerifyConnectionAsync(Task task) {
            if (task.IsCanceled || task.IsFaulted || task.Exception != null) {
                Volatile.Write(ref _refIsUsable, false);
                await RetryConnectionAsync();
            }
            else {
                Volatile.Write(ref _refIsUsable, true);
                _connectionAttempts = 0;
                OnConnected?.Invoke();
                await ReceiveAsync()
                    .ConfigureAwait(false);
            }
        }

        private async Task RetryConnectionAsync() {
            _tokenSource.Cancel(false);

            if (_connectionAttempts > _lavaConfig.ReconnectAttempts && _lavaConfig.ReconnectAttempts != -1)
                return;

            if (Volatile.Read(ref _refIsUsable))
                return;

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
                while (Volatile.Read(ref _refIsUsable) && _socket.State == WebSocketState.Open &&
                       !_tokenSource.IsCancellationRequested) {
                    var buffer = new byte[_lavaConfig.BufferSize];
                    var result = await _socket.ReceiveAsync(buffer, _tokenSource.Token)
                        .ConfigureAwait(false);

                    switch (result.MessageType) {
                        case WebSocketMessageType.Close:
                            Volatile.Write(ref _refIsUsable, false);
                            OnDisconnected?.Invoke("Server closed the connection!");

                            await RetryConnectionAsync()
                                .ConfigureAwait(false);
                            break;

                        case WebSocketMessageType.Text:
                            if (!result.EndOfMessage)
                                continue;

                            var startLength = buffer.Length - 1;
                            while (buffer[startLength] == 0)
                                --startLength;
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
    }
}