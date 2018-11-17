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
        private readonly string _name;
        private int _reconnectAttempts;
        private readonly Encoding _encoding;
        private Configuration _configuration;
        private ClientWebSocket _clientWebSocket;
        private readonly Func<LogMessage, Task> _log;

        public Func<string, bool> OnMessage;

        public SocketResolver(string nodeName, Configuration configuration, Func<LogMessage, Task> log)
        {
            _name = $"{nodeName}_Socket";
            _configuration = configuration;
            _log = log;
            _encoding = new UTF8Encoding(false);
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        }

        public async Task ConnectAsync()
        {
            _clientWebSocket = new ClientWebSocket
            {
                Options = {Proxy = _configuration.Proxy}
            };

            _clientWebSocket.Options.SetRequestHeader("User-Id", $"{_configuration.UserId}");
            _clientWebSocket.Options.SetRequestHeader("Num-Shards", $"{_configuration.Shards}");
            _clientWebSocket.Options.SetRequestHeader("Authorization", _configuration.Authorization);
            var url = new Uri($"ws://{_configuration.Host}:{_configuration.Port}");
            _log?.Invoke(LogResolver.Debug(_name, $"Connecting to {url}."));
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
            var data = _encoding.GetBytes(JsonConvert.SerializeObject(payload, Formatting.None));
            var segment = new ArraySegment<byte>(data);
            return _clientWebSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task VerifyConnectionAsync(Task task)
        {
            if (task.IsCanceled || task.IsFaulted || task.Exception != null)
            {
                _isUseable = false;
                _log?.Invoke(LogResolver.Debug(_name, "Websocket connection failed."));
                await RetryConnectionAsync().ConfigureAwait(false);
            }
            else
            {
                _isUseable = true;
                _reconnectAttempts = 0;
                _log?.Invoke(LogResolver.Debug(_name, "Websocket connection established."));
                await ReceiveAsync().ConfigureAwait(false);
            }
        }

        private async Task RetryConnectionAsync()
        {
            if (_reconnectAttempts > _configuration.ReconnectAttempts && _configuration.ReconnectAttempts != -1)
                return;

            if (_isUseable) return;
            _reconnectAttempts++;
            _interval += _configuration.ReconnectInterval;
            _log?.Invoke(LogResolver.Debug(_name,
                $"Retry attempt #{_reconnectAttempts}. Next retry in {_interval.Seconds}s."));
            await Task.Delay(_interval).ContinueWith(_ => ConnectAsync()).ConfigureAwait(false);
        }

        private async Task ReceiveAsync()
        {
            while (true)
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var buffer = new byte[_configuration.BufferSize];
                    var segment = new ArraySegment<byte>(buffer);
                    while (_clientWebSocket.State == WebSocketState.Open)
                    {
                        var result = await _clientWebSocket.ReceiveAsync(segment, CancellationToken.None)
                            .ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                            if (result.CloseStatus == WebSocketCloseStatus.EndpointUnavailable)
                            {
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