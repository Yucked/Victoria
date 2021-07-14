using System;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Victoria.WebSocket.EventArgs;

namespace Victoria.WebSocket {
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

        private readonly WebSocketConfiguration _webSocketConfiguration;
        private readonly ConcurrentQueue<byte[]> _messageQueue;
        private CancellationTokenSource _connectionTokenSource;
        private WebSocket _webSocket;
        private int _reconnectAttempts;
        private TimeSpan _reconnectDelay;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webSocketConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public WebSocketClient(WebSocketConfiguration webSocketConfiguration) {
            if (webSocketConfiguration == null) {
                throw new ArgumentNullException(nameof(webSocketConfiguration));
            }

            Host = new Uri(webSocketConfiguration.Endpoint);
            _webSocketConfiguration = webSocketConfiguration;
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

            (_webSocket as ClientWebSocket).Options.SetRequestHeader(key, value);
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

            try {
                await (_webSocket as ClientWebSocket)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bypassQueue"></param>
        /// <param name="serializerOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendAsync<T>(T data, bool bypassQueue = false,
                                       JsonSerializerOptions serializerOptions = default) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data), "Provided data was null.");
            }

            if (_webSocket.State != WebSocketState.Open) {
                throw new InvalidOperationException(
                    $"WebSocket is not in open state. Current state: {_webSocket.State}");
            }

            try {
                var serializedData = JsonSerializer.SerializeToUtf8Bytes(data, serializerOptions);
                if (bypassQueue) {
                    _messageQueue.Enqueue(serializedData);
                }
                else {
                    await _webSocket.SendAsync(serializedData, WebSocketMessageType.Text,
                        true, _connectionTokenSource.Token);
                }
            }
            catch (Exception exception) {
                await OnErrorAsync.Invoke(new ErrorEventArgs(exception));
            }
        }

        private async Task ReceiveAsync() {
            try {
                var buffer = new byte[_webSocketConfiguration.BufferSize];
                var finalBuffer = default(byte[]);
                var offset = 0;
                do {
                    var receiveResult = await _webSocket.ReceiveAsync(buffer, _connectionTokenSource.Token);
                    if (!receiveResult.EndOfMessage) {
                        finalBuffer = new byte[_webSocketConfiguration.BufferSize * 2];
                        buffer.CopyTo(finalBuffer, offset);
                        offset += receiveResult.Count;
                        buffer = new byte[_webSocketConfiguration.BufferSize];
                        continue;
                    }

                    switch (receiveResult.MessageType) {
                        case WebSocketMessageType.Text:
                            await OnDataAsync.Invoke(new DataEventArgs((finalBuffer ?? buffer).RemoveTrailingNulls()));

                            finalBuffer = default;
                            buffer = new byte[_webSocketConfiguration.BufferSize];
                            offset = 0;
                            break;

                        case WebSocketMessageType.Close:
                            await DisconnectAsync();
                            await ReconnectAsync();
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
            if (_webSocketConfiguration.ReconnectAttempts <= 0 ||
                _webSocketConfiguration.ReconnectAttempts <= _reconnectAttempts) {
                await OnRetryAsync.Invoke(new RetryEventArgs(0, true));
                return;
            }

            _connectionTokenSource.Cancel(false);
            _reconnectDelay += _webSocketConfiguration.ReconnectDelay;
            _reconnectAttempts++;

            await OnRetryAsync.Invoke(new RetryEventArgs(_reconnectAttempts, false));
            await Task.Delay(_reconnectDelay);
            await ConnectAsync();
        }

        private void ResetWebSocket() {
            var options = (_webSocket as ClientWebSocket).Options;
            var headerCollection = options.GetType()
                    .GetProperty("RequestHeaders", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(options, null)
                as WebHeaderCollection;

            _webSocket = new ClientWebSocket();
            foreach (var key in headerCollection.Keys) {
                (_webSocket as ClientWebSocket).Options.SetRequestHeader($"{key}", headerCollection.Get($"{key}"));
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
}