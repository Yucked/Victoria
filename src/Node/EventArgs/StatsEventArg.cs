using System;
using System.Text.Json.Serialization;
using Victoria.Converters;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Victoria.Node.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public struct StatsEventArg {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("playingPlayers"), JsonInclude]
        public int PlayingPlayers { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("players"), JsonInclude]
        public int Players { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("uptime"), JsonConverter(typeof(LongToTimeSpanConverter)), JsonInclude]
        public TimeSpan Uptime { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("memory"), JsonInclude]
        public Memory Memory { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cpu"), JsonInclude]
        public Cpu Cpu { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("frames"), JsonInclude]
        public Frames Frames { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Memory {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("reservable"), JsonInclude]
        public ulong Reservable { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("used"), JsonInclude]
        public ulong Used { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("allocated"), JsonInclude]
        public ulong Allocated { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("free"), JsonInclude]
        public ulong Free { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Cpu {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cores"), JsonInclude]
        public int Cores { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("systemLoad"), JsonInclude]
        public double SystemLoad { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("lavalinkLoad"), JsonInclude]
        public double LavalinkLoad { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Frames {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("sent"), JsonInclude]
        public int Sent { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("nulled"), JsonInclude]
        public int Nulled { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("deficit"), JsonInclude]
        public int Deficit { get; private set; }
    }
}