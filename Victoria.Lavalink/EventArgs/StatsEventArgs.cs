using System;
using Victoria.Lavalink.Responses.WebSocket;

namespace Victoria.Lavalink.EventArgs
{
    /// <summary>
    ///     Information about Lavalink statistics.
    /// </summary>
    public sealed class StatsEventArgs
    {
        /// <summary>
        ///     Connected players.
        /// </summary>
        public int Players { get; }

        /// <summary>
        ///     Players that are currently playing.
        /// </summary>
        public int PlayingPlayers { get; }

        /// <summary>
        ///     Lavalink uptime.
        /// </summary>
        public TimeSpan Uptime { get; }

        /// <summary>
        ///     General memory information about Lavalink.
        /// </summary>
        public Memory Memory { get; }

        /// <summary>
        ///     Audio frames.
        /// </summary>
        public Frames Frames { get; }

        /// <summary>
        ///     Machine's CPU info.
        /// </summary>
        public Cpu Cpu { get; }

        internal StatsEventArgs(StatsResponse response)
        {
            Players = response.Players;
            PlayingPlayers = response.PlayingPlayers;
            Uptime = new TimeSpan(response.Uptime);
            Memory = response.Memory;
            Frames = response.Frames;
            Cpu = response.Cpu;
        }
    }

    /// <summary>
    ///     General memory information about Lavalink.
    /// </summary>
    public struct Memory
    {
        /// <summary>
        ///     Memory used by Lavalink.
        /// </summary>
        public long Used { get; private set; }

        /// <summary>
        ///     Some JAVA stuff.
        /// </summary>
        public long Free { get; private set; }

        /// <summary>
        ///     Memory allocated by Lavalink.
        /// </summary>
        public long Allocated { get; private set; }

        /// <summary>
        ///     Reserved memory?
        /// </summary>
        public long Reservable { get; private set; }
    }

    /// <summary>
    ///     Audio frames.
    /// </summary>
    public struct Frames
    {
        /// <summary>
        ///     Audio frames sent.
        /// </summary>
        public int Sent { get; private set; }

        /// <summary>
        ///     Frames that were null.
        /// </summary>
        public int Nulled { get; private set; }

        /// <summary>
        ///     Frame deficit.
        /// </summary>
        public int Deficit { get; private set; }
    }

    /// <summary>
    ///     Machine's CPU info.
    /// </summary>
    public struct Cpu
    {
        /// <summary>
        ///     CPU Cores.
        /// </summary>
        public int Cores { get; private set; }

        /// <summary>
        ///     General load on CPU.
        /// </summary>
        public double SystemLoad { get; private set; }

        /// <summary>
        ///     Lavalink process load on CPU.
        /// </summary>
        public double LavalinkLoad { get; private set; }
    }
}