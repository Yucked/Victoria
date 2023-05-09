using System.Text.Json.Serialization;

namespace Victoria.Rest.Lavalink; 

/// <summary>
/// 
/// </summary>
/// <param name="SemVersion"></param>
/// <param name="Major"></param>
/// <param name="Minor"></param>
/// <param name="Patch"></param>
/// <param name="PreRelease"></param>
public readonly record struct LavalinkVersion(
    [property: JsonPropertyName("semver")] string SemVersion,
    [property: JsonPropertyName("major")] int Major,
    [property: JsonPropertyName("minor")] int Minor,
    [property: JsonPropertyName("patch")] int Patch,
    [property: JsonPropertyName("preRelease")] string PreRelease);