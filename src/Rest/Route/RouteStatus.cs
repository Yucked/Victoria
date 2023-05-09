using System.Text.Json.Serialization;

namespace Victoria.Rest.Route;

/// <summary>
/// 
/// </summary>
/// <param name="PlannerType"></param>
/// <param name="Detail"></param>
public readonly record struct RouteStatus(
    [property: JsonPropertyName("class")]
    RoutePlannerType PlannerType,
    [property: JsonPropertyName("details")]
    RouteDetail Detail);