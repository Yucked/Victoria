using System;
using System.Text.Json.Serialization;
using Victoria.Converters;

namespace Victoria.EventArgs {
    /// <summary>
    ///     Information about Lavalink statistics.
    /// </summary>
    public sealed class StatsEventArgs {
        /// <summary>
        ///     Machine's CPU info.
        /// </summary>
        [JsonPropertyName("cpu"), JsonInclude]
        public Cpu Cpu { get; private set; }

        /// <summary>
        ///     Audio frames.
        /// </summary>
        [JsonPropertyName("frames"), JsonInclude]
        public Frames Frames { get; private set; }

        /// <summary>
        ///     General memory information about Lavalink.
        /// </summary>
        [JsonPropertyName("memory"), JsonInclude]
        public Memory Memory { get; private set; }

        /// <summary>
        ///     Connected players.
        /// </summary>
        [JsonPropertyName("players"), JsonInclude]
        public int Players { get; private set; }

        /// <summary>
        ///     Players that are currently playing.
        /// </summary>
        [JsonPropertyName("playingPlayers"), JsonInclude]
        public int PlayingPlayers { get; private set; }

        /// <summary>
        ///     Lavalink uptime.
        /// </summary>
        [JsonPropertyName("uptime"), JsonConverter(typeof(LongToTimeSpanConverter)), JsonInclude]
        public TimeSpan Uptime { get; private set; }
    }

    /// <summary>
    ///     General memory information about Lavalink.
    /// </summary>
    public struct Memory {
        /// <summary>
        ///     Memory used by Lavalink.
        /// </summary>
        [JsonPropertyName("used"), JsonInclude]
        public ulong Used { get; private set; }

        /// <summary>
        ///     Some JAVA stuff.
        /// </summary>
        [JsonPropertyName("free"), JsonInclude]
        public ulong Free { get; private set; }

        /// <summary>
        ///     Memory allocated by Lavalink.
        /// </summary>
        [JsonPropertyName("allocated"), JsonInclude]
        public ulong Allocated { get; private set; }

        /// <summary>
        ///     Reserved memory?
        /// </summary>
        [JsonPropertyName("reservable"), JsonInclude]
        public ulong Reservable { get; private set; }
    }

    /// <summary>
    ///     Audio frames.
    /// </summary>
    public struct Frames {
        /// <summary>
        ///     Audio frames sent.
        /// </summary>
        [JsonPropertyName("sent"), JsonInclude]
        public int Sent { get; private set; }

        /// <summary>
        ///     Frames that were null.
        /// </summary>
        [JsonPropertyName("nulled"), JsonInclude]
        public int Nulled { get; private set; }

        /// <summary>
        ///     Frame deficit.
        /// </summary>
        [JsonPropertyName("deficit"), JsonInclude]
        public int Deficit { get; private set; }
    }

    /// <summary>
    ///     Machine's CPU info.
    /// </summary>
    public struct Cpu {
        /// <summary>
        ///     CPU Cores.
        /// </summary>
        [JsonPropertyName("cores"), JsonInclude]
        public int Cores { get; private set; }

        /// <summary>
        ///     General load on CPU.
        /// </summary>
        [JsonPropertyName("systemLoad"), JsonInclude]
        public double SystemLoad { get; private set; }

        /// <summary>
        ///     Lavalink process load on CPU.
        /// </summary>
        [JsonPropertyName("lavalinkLoad"), JsonInclude]
        public double LavalinkLoad { get; private set; }
    }
}