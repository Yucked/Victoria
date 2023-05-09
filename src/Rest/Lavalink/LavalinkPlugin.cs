using System.Text.Json.Serialization;

namespace Victoria.Rest.Lavalink; 

/// <summary>
/// 
/// </summary>
/// <param name="Name"></param>
/// <param name="Version"></param>
public readonly record struct LavalinkPlugin(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version);