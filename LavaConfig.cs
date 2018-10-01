using Discord;

namespace Victoria
{
    public struct LavaConfig
    {
        /// <summary>
        /// Number Of Tries For Reconnects.
        /// </summary>
        public int MaxTries { internal get; set; }

        /// <summary>
        /// Lavalink Authorization.
        /// </summary>
        public string Authorization { internal get; set; }

        /// <summary>
        /// Rest Hostname And Port.
        /// </summary>
        public Endpoint Rest { internal get; set; }

        /// <summary>
        /// Websocket Hostname And Port.
        /// </summary>
        public Endpoint Socket { internal get; set; }

        /// <summary>
        /// Get Severity of Log
        /// </summary>
        public LogSeverity Severity { internal get; set; }


        internal static LavaConfig Default => new LavaConfig
        {
            MaxTries = 0,
            Rest = new Endpoint
            {
                Port = 2333,
                Host = "127.0.0.1"
            },
            Socket = new Endpoint
            {
                Port = 80,
                Host = "127.0.0.1"
            },
            Severity = LogSeverity.Verbose,
            Authorization = "youshallnotpass"
        };
    }
}