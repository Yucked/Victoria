using Discord;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace Victoria.Helpers
{
    internal sealed class SocketHelper
    {
        private bool _isUseable;
        private TimeSpan _interval;
        private int _reconnectAttempts;
        private ClientWebSocket _clientWebSocket;
        private CancellationTokenSource _cancellationTokenSource;

        public SocketHelper()
        {
            ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) => true;
        }

        public async Task ConnectAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _clientWebSocket = new ClientWebSocket();

            _clientWebSocket.Options.SetRequestHeader("User-Id", $"{_configuration.UserId}");
            _clientWebSocket.Options.SetRequestHeader("Num-Shards", $"{_configuration.Shards}");
            _clientWebSocket.Options.SetRequestHeader("Authorization", _configuration.Authorization);
            var url = new Uri($"ws://{_configuration.Host}:{_configuration.Port}");
            try
            {
                await _clientWebSocket.ConnectAsync(url, CancellationToken.None).ContinueWith(VerifyConnectionAsync);
            }
            catch
            {
                // Ignore all websocket exceptions.
            }
        }
    }
}