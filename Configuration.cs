using System;
using System.Net;
using Discord;
using Discord.Rest;

namespace Victoria
{
    public struct Configuration
    {
        /// <summary>
        /// Number of <see cref="BaseDiscordClient"/> shards.
        /// </summary>
        internal int Shards { get; set; }

        /// <summary>
        /// User Id of <see cref="BaseDiscordClient"/>.
        /// </summary>
        internal ulong UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Number of reconnect attempts for websocket connection. Set to -1 for unlimited attempts.
        /// </summary>
        public int ReconnectAttempts { get; set; }

        /// <summary>
        /// Wait time before trying again.
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; }

        /// <summary>
        /// Websocket buffer size for receiving data.
        /// </summary>
        public ushort BufferSize { get; set; }

        /// <summary>
        /// Websocket and Rest hostname of Lavalink server.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Websocket and Rest port of Lavalink server.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Lavalink server authorization.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Logging severity of everything.
        /// </summary>
        public LogSeverity Severity { get; set; }

        /// <summary>
        /// Voice channel connection options.
        /// </summary>
        public VoiceChannelOptions VoiceChannelOptions { get; set; }

        /// <summary>
        /// Default configuration.
        /// </summary>
        internal static Configuration Default
            => new Configuration
            {
                ReconnectAttempts = 10,
                ReconnectInterval = TimeSpan.FromSeconds(3000),
                BufferSize = 1024,
                Authorization = "youshallnotpass",
                Host = "127.0.0.1",
                Port = 2333,
                Severity = LogSeverity.Info,
                VoiceChannelOptions = VoiceChannelOptions.Default
            };
    }

    /// <summary>
    /// Voice channel connection options.
    /// </summary>
    public struct VoiceChannelOptions
    {
        /// <summary>
        /// Make <see cref="BaseDiscordClient"/> deaf.
        /// </summary>
        public bool SelfDeaf { get; set; }

        /// <summary>
        /// Make <see cref="BaseDiscordClient"/> mute.
        /// </summary>
        public bool SelfMute { get; set; }

        /// <summary>
        /// OwO, what's this?
        /// </summary>
        public bool External { get; set; }

        internal static VoiceChannelOptions Default
            => new VoiceChannelOptions
            {
                External = false,
                SelfDeaf = true,
                SelfMute = false
            };
    }
}