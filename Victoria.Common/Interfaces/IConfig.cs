using System;
using Discord;

namespace Victoria.Common.Interfaces
{
    /// <summary>
    ///     Represents basic configuration.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        ///     Port to connect to.
        /// </summary>
        ushort Port { get; set; }

        /// <summary>
        ///     Server's IP/Hostname.
        /// </summary>
        string Hostname { get; set; }

        /// <summary>
        ///     Server's password/authentication.
        /// </summary>
        string Authorization { get; set; }

        /// <summary>
        ///     Whether to enable self deaf for bot.
        /// </summary>
        bool SelfDeaf { get; set; }

        /// <summary>
        ///     Reconnection delay for retrying websocket connection.
        /// </summary>
        TimeSpan ReconnectDelay { get; set; }

        /// <summary>
        ///     How many reconnect attempts are allowed.
        /// </summary>
        int ReconnectAttempts { get; set; }

        /// <summary>
        ///     Log serverity for logging everything.
        /// </summary>
        LogSeverity LogSeverity { get; set; }

        /// <summary>
        ///     Max buffer size for receiving websocket message.
        /// </summary>
        ushort BufferSize { get; set; }
    }
}