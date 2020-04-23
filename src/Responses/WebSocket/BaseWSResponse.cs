using System;
using Victoria.Enums;
using Victoria.EventArgs;

namespace Victoria.Responses.WebSocket {
	internal class BaseWsResponse {
	}

	internal sealed class PlayerUpdateResponse : BaseWsResponse {
		public ulong GuildId { get; set; }
		public PlayerState State { get; set; }
	}

	internal struct PlayerState {
		public DateTimeOffset Time { get; set; }
		public TimeSpan Position { get; set; }
	}

	internal sealed class StatsResponse : BaseWsResponse {
		public Cpu Cpu { get; set; }
		public Frames Frames { get; set; }
		public Memory Memory { get; set; }
		public int Players { get; set; }
		public int PlayingPlayers { get; set; }
		public long Uptime { get; set; }
	}

	internal class BaseEventResponse : BaseWsResponse {
		public ulong GuildId { get; set; }
	}

	internal sealed class TrackStartEvent : BaseEventResponse {
		public string Hash { get; set; }
	}

	internal sealed class TrackEndEvent : BaseEventResponse {
		public string Hash { get; set; }
		public TrackEndReason Reason { get; set; }
	}

	internal sealed class TrackExceptionEvent : BaseEventResponse {
		public string Error { get; set; }
		public string Hash { get; set; }
	}

	internal sealed class TrackStuckEvent : BaseEventResponse {
		public string Hash { get; set; }
		public long ThresholdMs { get; set; }
	}

	internal sealed class WebSocketClosedEvent : BaseEventResponse {
		public bool ByRemote { get; set; }
		public int Code { get; set; }
		public string Reason { get; set; }
	}
}