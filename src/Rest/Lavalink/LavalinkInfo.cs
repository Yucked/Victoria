using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Victoria.Rest.Lavalink;

/// <summary>
/// 
/// </summary>
/// <param name="Version"></param>
/// <param name="BuildTime"></param>
/// <param name="Git"></param>
/// <param name="JavaVirtualMachine"></param>
/// <param name="LavaplayerVersion"></param>
/// <param name="SourceManagers"></param>
/// <param name="Filters"></param>
/// <param name="Plugins"></param>
public readonly record struct LavalinkInfo(
    [property: JsonPropertyName("version")]
    LavalinkVersion Version,
    [property: JsonPropertyName("buildTime")]
    int BuildTime,
    [property: JsonPropertyName("git")]
    LavalinkGit Git,
    [property: JsonPropertyName("jvm")]
    string JavaVirtualMachine,
    [property: JsonPropertyName("lavaplayer")]
    string LavaplayerVersion,
    [property: JsonPropertyName("sourceManagers")]
    IEnumerable<string> SourceManagers,
    [property: JsonPropertyName("filters")]
    IEnumerable<Filters.Filters> Filters,
    [property: JsonPropertyName("plugins")] IEnumerable<LavalinkPlugin> Plugins);