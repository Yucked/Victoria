using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using Victoria.Entities.Payloads;

namespace Victoria.Utilities
{
    internal sealed class SocketResolver
    {
        private bool _isUseable;
        private TimeSpan _interval;
        private int _reconnectAttempts;
        private readonly Encoding _encoding;
        private Configuration _configuration;
        private ClientWebSocket _clientWebSocket;
        private readonly Func<LogMessage, Task> _log;
        private readonly (string socket, string node) _name;
        private CancellationTokenSource _cancellationTokenSource;

        public Func<string, bool> OnMessage;

        public SocketResolver(string nodeName, Configuration configuration, Func<LogMessage, Task> log)
        {
            _log = log;
            _name.node = nodeName;
            _configuration = configuration;
            _name.socket = $"{nodeName}-Socket";
            _encoding = new UTF8Encoding(false);
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        }

        public async Task ConnectAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _clientWebSocket = new ClientWebSocket
            {
                Options = {Proxy = _configuration.Proxy}
            };

            if (_configuration.EnableResuming)
                _clientWebSocket.Options.SetRequestHeader("Resume-Key", $"{_name.node}-Resume");

            _clientWebSocket.Options.SetRequestHeader("User-Id", $"{_configuration.UserId}");
            _clientWebSocket.Options.SetRequestHeader("Num-Shards", $"{_configuration.Shards}");
            _clientWebSocket.Options.SetRequestHeader("Authorization", _configuration.Authorization);
            var url = new Uri($"ws://{_configuration.Host}:{_configuration.Port}");
            _log?.Invoke(LogResolver.Info(_name.socket, $"Connecting to {url}."));
            try
            {
                await _clientWebSocket.ConnectAsync(url, CancellationToken.None).ContinueWith(VerifyConnectionAsync);
            }
            catch
            {
                // Ignore all websocket exceptions.
            }
        }

        public async Task DisconnectAsync()
        {
            await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closed called.",
                CancellationToken.None).ConfigureAwait(false);
            await _clientWebSocket
                .CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed called.", CancellationToken.None)
                .ConfigureAwait(false);
            _isUseable = false;
        }

        public Task SendPayloadAsync(LavaPayload payload)
        {
            if (!_isUseable)
                return Task.CompletedTask;
            var convert = JsonConvert.SerializeObject(payload);
            var bytes = _encoding.GetBytes(convert);
            var segment = new ArraySegment<byte>(bytes);
            return _clientWebSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task VerifyConnectionAsync(Task task)
        {
            if (task.IsCanceled || task.IsFaulted || task.Exception != null)
            {
                _isUseable = false;
                _log?.Invoke(LogResolver.Error(_name.socket, "Websocket connection failed."));
                await RetryConnectionAsync().ConfigureAwait(false);
            }
            else
            {
                _isUseable = true;
                _reconnectAttempts = 0;
                _log?.Invoke(LogResolver.Info(_name.socket, "Websocket connection established."));
                await ReceiveAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        private async Task RetryConnectionAsync()
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch
            {
                // Ignored.
            }

            if (_reconnectAttempts > _configuration.ReconnectAttempts && _configuration.ReconnectAttempts != -1)
                return;

            if (_isUseable)
                return;
            _reconnectAttempts++;
            _interval += _configuration.ReconnectInterval;
            _log?.Invoke(LogResolver.Info(_name.socket,
                $"Retry attempt #{_reconnectAttempts}. Next retry in {_interval.Seconds}s."));
            await Task.Delay(_interval).ContinueWith(_ => ConnectAsync()).ConfigureAwait(false);
        }

        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    byte[] bytes;
                    using (var stream = new MemoryStream())
                    {
                        var buffer = new byte[_configuration.BufferSize];
                        var segment = new ArraySegment<byte>(buffer);
                        while (_clientWebSocket.State == WebSocketState.Open)
                        {
                            var result = await _clientWebSocket.ReceiveAsync(segment, cancellationToken)
                                .ConfigureAwait(false);
                            if (result.MessageType == WebSocketMessageType.Close)
                                if (result.CloseStatus == WebSocketCloseStatus.EndpointUnavailable)
                                {
                                    _isUseable = false;
                                    await RetryConnectionAsync().ConfigureAwait(false);
                                    break;
                                }

                            stream.Write(buffer, 0, result.Count);
                            if (result.EndOfMessage)
                                break;
                        }

                        bytes = stream.ToArray();
                    }

                    if (bytes.Length <= 0)
                        continue;

                    var parse = _encoding.GetString(bytes).Trim('\0');
                    OnMessage(parse);
                }
            }
            catch (Exception ex) when (ex.HResult == -2147467259)
            {
                _isUseable = false;
                _log?.Invoke(LogResolver.Error(_name.socket, ex.Message, ex));
                await RetryConnectionAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _isUseable = false;
            _reconnectAttempts = 0;
            _clientWebSocket.Dispose();
            _clientWebSocket = null;
            _configuration = default;
        }
    }
}