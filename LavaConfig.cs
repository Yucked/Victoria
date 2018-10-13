using Discord;

namespace Victoria
{
    public struct LavaConfig
    {
        /// <summary>
        /// Max number of connection re-attempts
        /// </summary>
        public int MaxTries { get; set; }
        /// <summary>
        ///     Lavalink Authorization.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        ///     Rest Hostname And Port.
        /// </summary>
        public Endpoint Rest { get; set; }

        /// <summary>
        ///     Websocket Hostname And Port.
        /// </summary>
        public Endpoint Socket { get; set; }

        /// <summary>
        ///     Get Severity of Log
        /// </summary>
        public LogSeverity Severity { get; set; }


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
            Severity = LogSeverity.Debug,
            Authorization = "youshallnotpass"
        };
    }
}