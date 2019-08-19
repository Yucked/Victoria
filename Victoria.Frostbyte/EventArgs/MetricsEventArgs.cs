using System;
using Newtonsoft.Json;
using Victoria.Frostbyte.Infos.Metrics;

namespace Victoria.Frostbyte.EventArgs
{
    /// <summary>
    /// </summary>
    public struct MetricsEventArgs
    {
        [JsonProperty("uptime")]
        private long RawUptime { get; set; }

        /// <summary>
        ///     Process uptime.
        /// </summary>
        public TimeSpan Uptime
            => new TimeSpan(RawUptime);

        /// <summary>
        /// </summary>
        public int PlayingPlayers { get; private set; }

        /// <summary>
        /// </summary>
        public int ConnectedClients { get; private set; }

        /// <summary>
        /// </summary>
        public int ConnectedPlayers { get; private set; }

        /// <summary>
        /// </summary>
        public CpuInfo CpuInfo { get; private set; }

        /// <summary>
        /// </summary>
        public MemoryInfo MemoryInfo { get; private set; }
    }
}
