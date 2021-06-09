using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Victoria.Responses.Route {
    /// <summary>
    /// </summary>
    public sealed class RouteDetail {
        /// <summary>
        ///     Returns information about IP address.
        /// </summary>
        [JsonPropertyName("ipBlock"), JsonInclude]
        public RouteIPBlock IPBlock { get; private set; }

        /// <summary>
        ///     List of IP addresses that are failing.
        /// </summary>
        [JsonPropertyName("failingAddresses"), JsonInclude]
        public ICollection<RouteFailedAddresses> FailedAddresses { get; private set; }

        /// <summary>
        ///     Information in which /64 block ips are chosen. This number increases on each ban.
        ///     <para>
        ///         Only available if RouteStatus.Class is set to RotatingNanoIpRoutePlanner.
        ///     </para>
        /// </summary>
        [JsonPropertyName("blockIndex"), JsonInclude]
        public string BlockIndex { get; private set; }

        /// <summary>
        ///     Current offset in the ip block.
        ///     <para>
        ///         Only available if RouteStatus.Class is set to RotatingNanoIpRoutePlanner / NanoIpRoutePlanner.
        ///     </para>
        /// </summary>
        [JsonPropertyName("currentAddressIndex"), JsonInclude]
        public string CurrentAddressIndex { get; private set; }

        /// <summary>
        ///     Number of rotations which happened since the restart of Lavalink.
        ///     <para>
        ///         Only available if RouteStatus.Class is set to RotatingIpRoutePlanner.
        ///     </para>
        /// </summary>
        [JsonPropertyName("routeIndex"), JsonInclude]
        public string RotateIndex { get; internal set; }

        /// <summary>
        ///     Current offset in the block.
        ///     <para>
        ///         Only available if RouteStatus.Class is set to RotatingIpRoutePlanner.
        ///     </para>
        /// </summary>
        [JsonPropertyName("ipIndex"), JsonInclude]
        public string IPIndex { get; internal set; }

        /// <summary>
        ///     The currently used ip address
        ///     <para>
        ///         Only available if RouteStatus.Class is set to RotatingIpRoutePlanner.
        ///     </para>
        /// </summary>
        [JsonPropertyName("currentAddress"), JsonInclude]
        public string CurrentAddress { get; internal set; }
    }
}