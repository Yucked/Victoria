using Discord;
using Newtonsoft.Json;
using PureWebSockets;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using Victoria.Payloads;

namespace Victoria
{
    public sealed class LavaSocket
    {
        private bool IsDisposed;
        private readonly LavaConfig _config;
        internal readonly Lavalink Lavalink;
        internal PureWebSocket PureSocket { get; }
        internal bool IsConnected => !Volatile.Read(ref IsDisposed);


        internal LavaSocket(LavaConfig config, Lavalink lavalink, int shards, ulong userId)
        {
            _config = config;
            Lavalink = lavalink;
            var socket = new PureWebSocket($"ws://{config.Socket.Host}:{config.Socket.Port}",
                new PureWebSocketOptions
                {
                    DebugMode = config.Severity == LogSeverity.Debug,
                    Headers = new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("Num-Shards", $"{shards}"),
                        new Tuple<string, string>("Authorization", config.Authorization),
                        new Tuple<string, string>("User-Id", $"{userId}")
                    },
                    IgnoreCertErrors = true,
                    MyReconnectStrategy = new ReconnectStrategy(reconnectInterval: 100, config.MaxTries)
                });

            socket.OnError += OnError;
            socket.OnClosed += OnClosed;
            socket.OnFatality += OnFatality;
            socket.OnOpened += OnOpened;
            socket.OnSendFailed += OnSendFailed;
            socket.OnStateChanged += OnStateChanged;
            PureSocket = socket;
        }

        internal void Connect()
        {
            try
            {
                switch (_config.Severity)
                {
                    case LogSeverity.Debug:
                    case LogSeverity.Verbose:
                    case LogSeverity.Info:
                        Lavalink.InvokeLog(_config.Severity, "Connecting to websocket.");
                        break;
                }

                var check = PureSocket.Connect();
                if (!check)
                {
                    switch (_config.Severity)
                    {
                        case LogSeverity.Debug:
                        case LogSeverity.Verbose:
                        case LogSeverity.Info:
                            Lavalink.InvokeLog(_config.Severity, "Failed to connect to websocket.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                switch (_config.Severity)
                {
                    case LogSeverity.Debug:
                    case LogSeverity.Critical:
                    case LogSeverity.Error:
                    case LogSeverity.Warning:
                    case LogSeverity.Verbose:
                        Lavalink.InvokeLog(_config.Severity, null, ex);
                        break;
                }
            }
        }

        internal void Disconnect()
        {
            PureSocket.Disconnect();
            PureSocket.Dispose(true);
            Volatile.Write(ref IsDisposed, true);
        }

        internal void SendPayload(LavaPayload load)
        {
            PureSocket.Send(JsonConvert.SerializeObject(load));
            switch (_config.Severity)
            {
                case LogSeverity.Debug:
                    Lavalink.InvokeLog(_config.Severity, JsonConvert.SerializeObject(load));
                    break;
                case LogSeverity.Verbose:
                    Lavalink.InvokeLog(_config.Severity, $"Sent {load.Operation} payload.");
                    break;
            }
        }

        private void OnClosed(WebSocketCloseStatus reason)
        {
            switch (_config.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Debug:
                case LogSeverity.Warning:
                case LogSeverity.Error:
                    switch (reason)
                    {
                        case WebSocketCloseStatus.EndpointUnavailable:
                        case WebSocketCloseStatus.PolicyViolation:
                        case WebSocketCloseStatus.ProtocolError:
                        case WebSocketCloseStatus.InternalServerError:
                        case WebSocketCloseStatus.InvalidMessageType:
                        case WebSocketCloseStatus.InvalidPayloadData:
                        case WebSocketCloseStatus.MessageTooBig:
                            Lavalink.InvokeLog(_config.Severity, $"{reason}");
                            break;
                    }

                    break;
            }
        }

        private void OnOpened()
        {
            Volatile.Write(ref IsDisposed, false);
            switch (_config.Severity)
            {
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                case LogSeverity.Info:
                    Lavalink.InvokeLog(_config.Severity, "Socket connection established.");
                    break;
            }
        }

        private void OnError(Exception ex)
        {
            switch (_config.Severity)
            {
                case LogSeverity.Debug:
                case LogSeverity.Error:
                case LogSeverity.Critical:
                case LogSeverity.Warning:
                    Lavalink.InvokeLog(_config.Severity, null, ex);
                    break;
            }
        }

        private void OnFatality(string reason)
        {
            switch (_config.Severity)
            {
                case LogSeverity.Debug:
                case LogSeverity.Error:
                case LogSeverity.Critical:
                case LogSeverity.Warning:
                    Lavalink.InvokeLog(_config.Severity, reason);
                    break;
            }
        }

        private void OnSendFailed(string data, Exception ex)
        {
            switch (_config.Severity)
            {
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    Lavalink.InvokeLog(_config.Severity, data, ex);
                    break;

                case LogSeverity.Info:
                    Lavalink.InvokeLog(_config.Severity, "Failed to send data.");
                    break;

                case LogSeverity.Error:
                case LogSeverity.Critical:
                case LogSeverity.Warning:
                    Lavalink.InvokeLog(_config.Severity, null, ex);
                    break;
            }
        }

        private void OnStateChanged(WebSocketState newstate, WebSocketState prevstate)
        {
            switch (_config.Severity)
            {
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    Lavalink.InvokeLog(LogSeverity.Debug, $"State Changed: {prevstate} -> {newstate}");
                    break;
            }
        }
    }
}