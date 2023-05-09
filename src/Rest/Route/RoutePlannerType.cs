namespace Victoria.Rest.Route;

/// <summary>
/// RoutePlanner implementation being used by this server
/// </summary>
public enum RoutePlannerType {
    /// <summary>
    /// IP address used is switched on ban. Recommended for IPv4 blocks or IPv6 blocks smaller than a /64.
    /// </summary>
    RotatingIpRoutePlanner,
    
    /// <summary>
    /// IP address used is switched on clock update. Use with at least 1 /64 IPv6 block.
    /// </summary>
    NanoIpRoutePlanner,
    
    /// <summary>
    /// IP address used is switched on clock update, rotates to a different /64 block on ban.
    /// Use with at least 2x /64 IPv6 blocks.
    /// </summary>
    RotatingNanoIpRoutePlanner,
    
    /// <summary>
    /// IP address used is selected at random per request. Recommended for larger IP blocks.
    /// </summary>
    BalancingIpRoutePlanner
}