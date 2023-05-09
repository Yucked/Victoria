using System.Text.Json.Serialization;

namespace Victoria.Rest.Route;

/// <summary>
/// 
/// </summary>
/// <param name="FailingAddress"></param>
/// <param name="FailingTimestamp"></param>
/// <param name="FailingTime"></param>
public readonly record struct RouteFailingAddress(
    [property: JsonPropertyName("failingAddress")]
    string FailingAddress,
    [property: JsonPropertyName("failingTimestamp")]
    int FailingTimestamp,
    [property: JsonPropertyName("failingTime")]
    string FailingTime);