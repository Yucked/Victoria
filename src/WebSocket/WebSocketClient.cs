using System;
using System.Collections.Concurrent;
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
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Uri Host { get; }

        private readonly WebSocket _webSocket;
        private readonly ConcurrentQueue<byte[]> _messageQueue;
        private CancellationTokenSource _connectionTokenSource;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="websocketAddress"></param>
        public WebSocketClient(Uri websocketAddress) :
            this(websocketAddress.Host, websocketAddress.Port, websocketAddress.Scheme) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="scheme"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public WebSocketClient(string hostname, int port, string scheme) {
            if (string.IsNullOrWhiteSpace(hostname)) {
                throw new ArgumentNullException(nameof(hostname),
                    "Hostname was not provided.");
            }

            if (port <= 0) {
                throw new ArgumentOutOfRangeException(nameof(port), "Invalid port provided.");
            }

            Host = new Uri($"{scheme}://{hostname}:{port}");
            _webSocket = new ClientWebSocket();
            _messageQueue = new ConcurrentQueue<byte[]>();
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
                    $"WebSocket is not in open state. Current state: {_webSocket.State}");
            }

            await (_webSocket as ClientWebSocket).ConnectAsync(Host, _connectionTokenSource.Token)
                .ContinueWith(async task => {
                    await task;
                    IsConnected = true;

                    _connectionTokenSource = new CancellationTokenSource();
                    await Task.WhenAll(OnOpenAsync.Invoke(), ReceiveAsync(), SendAsync());
                });
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
                var buffer = new byte[1024];
                var finalBuffer = default(byte[]);
                var offset = 0;
                do {
                    var receiveResult = await _webSocket.ReceiveAsync(buffer, _connectionTokenSource.Token);
                    if (!receiveResult.EndOfMessage) {
                        finalBuffer = new byte[2048];
                        buffer.CopyTo(finalBuffer, offset);
                        offset += receiveResult.Count;
                        buffer = new byte[1024];
                        continue;
                    }

                    switch (receiveResult.MessageType) {
                        case WebSocketMessageType.Text:
                            await OnDataAsync.Invoke(new DataEventArgs(finalBuffer));
                            break;

                        case WebSocketMessageType.Close:
                            await DisconnectAsync();
                            break;
                    }
                } while (_webSocket.State == WebSocketState.Open &&
                         !_connectionTokenSource.IsCancellationRequested);
            }
            catch (Exception exception) {
                if (exception is TaskCanceledException
                    || exception is OperationCanceledException
                    || exception is ObjectDisposedException) {
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