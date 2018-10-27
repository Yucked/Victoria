using HyperEx;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Misc;
using Victoria.Payloads;

namespace Victoria
{
    internal sealed class LavaSocket : IDisposable
    {
        private bool IsDisposed;
        private readonly LavaConfig _config;
        private readonly Encoding _encoding;
        private ClientWebSocket _webSocket;

        internal int _tries;
        internal event AsyncEvent OnClose;
        internal readonly Lavalink _lavalink;
        internal event Action<string> OnReceive;
        internal bool IsConnected => !Volatile.Read(ref IsDisposed);

        internal LavaSocket(LavaConfig config, Lavalink lavalink)
        {
            _config = config;
            _lavalink = lavalink;
            _encoding = new UTF8Encoding(false);
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        }

        public async Task ConnectAsync()
        {
            _webSocket = new ClientWebSocket();
            _webSocket.Options.SetRequestHeader("User-Id", $"{_config.UserId}");
            _webSocket.Options.SetRequestHeader("Num-Shards", $"{_config.Shards}");
            _webSocket.Options.SetRequestHeader("Authorization", _config.Authorization);
            _lavalink.LogInfo($"Connecting to Lavalink node at {_config.Endpoint.Host}:{_config.Endpoint.Port}");
            try
            {
                await _webSocket.ConnectAsync(new Uri($"ws://{_config.Endpoint.Host}:{_config.Endpoint.Port}"),
                    CancellationToken.None).ContinueWith(AfterConnect);
            }
            catch
            {
            }
        }

        private void AfterConnect(Task connectTask)
        {
            if (connectTask.IsCanceled || connectTask.IsFaulted || connectTask.Exception != null)
            {
                Volatile.Write(ref IsDisposed, true);
                OnClose?.Invoke();
            }
            else
                Task.Run(async () =>
                {
                    _tries = 0;
                    Volatile.Write(ref IsDisposed, false);
                    _lavalink.LogInfo("Connected to lavalink node.");
                    await ReceiveDataAsync();
                }, CancellationToken.None);
        }

        public async Task ReceiveDataAsync()
        {
            try
            {
                while (true)
                {
                    var data = await ReceiveAsync();
                    if (data.Length <= 0)
                        continue;
                    var message = _encoding.GetString(data);
                    var trimmed = message.Trim('\0');
                    OnReceive?.Invoke(trimmed);
                }
            }
            catch (OperationCanceledException opEx)
            {
                _lavalink.LogError("Server disconnected.", opEx);
            }
            catch (WebSocketException wsEx)
            {
                _lavalink.LogError("Websocket error.", wsEx);
            }
            catch (Exception ex)
            {
                _lavalink.LogError(null, ex);
            }
            finally
            {
                Volatile.Write(ref IsDisposed, true);
                OnClose?.Invoke();
            }
        }

        public async Task DisconnectAsync()
        {
            await _webSocket
                .CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Manual Closure.", CancellationToken.None)
                .ConfigureAwait(false);
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Manual closer.", CancellationToken.None)
                .ConfigureAwait(false);
            _webSocket.Dispose();
            Volatile.Write(ref IsDisposed, true);
        }

        public async Task SendAsync(string data)
        {
            var bytes = _encoding.GetBytes(data);
            await _webSocket
                .SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None)
                .ConfigureAwait(false);
        }

        private async Task<byte[]> ReceiveAsync()
        {
            using (var stream = new MemoryStream())
            {
                var buffer = new byte[1024];
                var segment = new ArraySegment<byte>(buffer);
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(segment, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        OnClose?.Invoke();
                        break;
                    }
                    else
                    {
                        stream.Write(buffer, 0, result.Count);
                        if (result.EndOfMessage) break;
                    }
                }
                return stream.ToArray();
            }
        }

        internal void SendPayload(LavaPayload load)
        {
            var data = JsonConvert.SerializeObject(load);
            Asyncs.RunSync(() => SendAsync(data));
            _lavalink.LogDebug(JsonConvert.SerializeObject(load));
        }

        public void Dispose()
        {
            _webSocket.Dispose();
            _webSocket = null;
        }
    }
}