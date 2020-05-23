using System;
using Victoria.Responses.WebSocket;

namespace Victoria.EventArgs {
	/// <summary>
	///     Information about Lavalink statistics.
	/// </summary>
	public sealed class StatsEventArgs {
		/// <summary>
		///     Machine's CPU info.
		/// </summary>
		public Cpu Cpu { get; }

		/// <summary>
		///     Audio frames.
		/// </summary>
		public Frames Frames { get; }

		/// <summary>
		///     General memory information about Lavalink.
		/// </summary>
		public Memory Memory { get; }

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

		internal StatsEventArgs(StatsResponse response) {
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
	public struct Memory {
		/// <summary>
		///     Memory used by Lavalink.
		/// </summary>
		public long Used { get; internal set; }

		/// <summary>
		///     Some JAVA stuff.
		/// </summary>
		public long Free { get; internal set; }

		/// <summary>
		///     Memory allocated by Lavalink.
		/// </summary>
		public long Allocated { get; internal set; }

		/// <summary>
		///     Reserved memory?
		/// </summary>
		public long Reservable { get; internal set; }
	}

	/// <summary>
	///     Audio frames.
	/// </summary>
	public struct Frames {
		/// <summary>
		///     Audio frames sent.
		/// </summary>
		public int Sent { get; internal set; }

		/// <summary>
		///     Frames that were null.
		/// </summary>
		public int Nulled { get; internal set; }

		/// <summary>
		///     Frame deficit.
		/// </summary>
		public int Deficit { get; internal set; }
	}

	/// <summary>
	///     Machine's CPU info.
	/// </summary>
	public struct Cpu {
		/// <summary>
		///     CPU Cores.
		/// </summary>
		public int Cores { get; internal set; }

		/// <summary>
		///     General load on CPU.
		/// </summary>
		public double SystemLoad { get; internal set; }

		/// <summary>
		///     Lavalink process load on CPU.
		/// </summary>
		public double LavalinkLoad { get; internal set; }
	}
}