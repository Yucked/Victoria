using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Victoria.Rest.Route;

/// <summary>
/// 
/// </summary>
/// <param name="IPBlock"></param>
/// <param name="FailingAddresses"></param>
/// <param name="RotateIndex"></param>
/// <param name="IPIndex"></param>
/// <param name="CurrentAddress"></param>
/// <param name="CurrentAddressIndex"></param>
/// <param name="BlockIndex"></param>
public readonly record struct RouteDetail(
    [property: JsonPropertyName("ipBlock")]
    RouteIPBlock IPBlock,
    [property: JsonPropertyName("failingAddresses")]
    ICollection<RouteFailingAddress> FailingAddresses,
    [property: JsonPropertyName("rotateIndex")]
    string RotateIndex,
    [property: JsonPropertyName("ipIndex")]
    string IPIndex,
    [property: JsonPropertyName("currentAddress")]
    string CurrentAddress,
    [property: JsonPropertyName("currentAddressIndex")]
    string CurrentAddressIndex,
    [property: JsonPropertyName("blockIndex")]
    string BlockIndex
);