using System.Text.Json.Serialization;

namespace Victoria.Rest.Route;

/// <summary>
/// 
/// </summary>
/// <param name="BlockType"></param>
/// <param name="Size"></param>
public readonly record struct RouteIPBlock(
    [property: JsonPropertyName("type")]
    RouteIPBlockType BlockType,
    [property: JsonPropertyName("size")]
    string Size);