using Discord;

namespace Victoria
{
    public struct LavaConfig
    {
        internal int Shards { get; set; }
        internal ulong UserId { get; set; }

        /// <summary>
        /// Specify a buffer size for websocket.
        /// </summary>
        public ushort BufferSize { get; set; }

        /// <summary>
        /// Max number of retry attempts.
        /// </summary>
        public int MaxTries { get; set; }

        /// <summary>
        /// Lavalink Authorization.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Rest Hostname And Port.
        /// </summary>
        public Endpoint Endpoint { get; set; }

        /// <summary>
        /// Get Severity of Log
        /// </summary>
        public LogSeverity Severity { get; set; }


        internal static LavaConfig Default => new LavaConfig
        {
            BufferSize = 2048,
            MaxTries = 15,
            Endpoint = new Endpoint
            {
                Port = 2333,
                Host = "127.0.0.1"
            },
            Severity = LogSeverity.Info,
            Authorization = "youshallnotpass"
        };
    }
}