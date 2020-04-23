using System.Collections.Generic;

namespace Victoria.Responses.Rest {
	/// <summary>
	/// </summary>
	public struct RouteDetail {
		/// <summary>
		///     Returns information about IP address.
		/// </summary>
		public IPBlock IPBlock { get; internal set; }

		/// <summary>
		///     List of IP addresses that are failing.
		/// </summary>
		public ICollection<FailedAddress> FailedAddresses { get; internal set; }

		/// <summary>
		///     Information in which /64 block ips are chosen. This number increases on each ban.
		///     <para>
		///         Only available if RouteStatus.Class is set to RotatingNanoIpRoutePlanner.
		///     </para>
		/// </summary>
		public string BlockIndex { get; internal set; }

		/// <summary>
		///     Current offset in the ip block.
		///     <para>
		///         Only available if RouteStatus.Class is set to RotatingNanoIpRoutePlanner / NanoIpRoutePlanner.
		///     </para>
		/// </summary>
		public string CurrentAddressIndex { get; internal set; }

		/// <summary>
		///     Number of rotations which happened since the restart of Lavalink.
		///     <para>
		///         Only available if RouteStatus.Class is set to RotatingIpRoutePlanner.
		///     </para>
		/// </summary>
		public string RotateIndex { get; internal set; }

		/// <summary>
		///     Current offset in the block.
		///     <para>
		///         Only available if RouteStatus.Class is set to RotatingIpRoutePlanner.
		///     </para>
		/// </summary>
		public string IPIndex { get; internal set; }

		/// <summary>
		///     The currently used ip address
		///     <para>
		///         Only available if RouteStatus.Class is set to RotatingIpRoutePlanner.
		///     </para>
		/// </summary>
		public string CurrentAddress { get; internal set; }
	}

	/// <summary>
	///     Represents the current IP address being used.
	/// </summary>
	public struct IPBlock {
		/// <summary>
		///     Type of IP address being used.
		/// </summary>
		public string Type { get; internal set; }

		/// <summary>
		///     Size of IP address?
		/// </summary>
		public string Size { get; internal set; }
	}

	/// <summary>
	///     Address that is failing.
	/// </summary>
	public struct FailedAddress {
		/// <summary>
		///     IP Address
		/// </summary>
		public string Address { get; internal set; }

		/// <summary>
		///     UNIX Epoch representation of timestamp
		/// </summary>
		public long Timestamp { get; internal set; }

		/// <summary>
		///     Time when this address failed.
		/// </summary>
		public string FailedOn { get; internal set; }
	}
}