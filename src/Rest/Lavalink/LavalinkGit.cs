using System.Text.Json.Serialization;

namespace Victoria.Rest.Lavalink; 

/// <summary>
/// 
/// </summary>
/// <param name="Branch"></param>
/// <param name="Commit"></param>
/// <param name="CommitTime"></param>
public readonly record struct LavalinkGit(
    [property: JsonPropertyName("branch")] string Branch,
    [property: JsonPropertyName("commit")] string Commit,
    [property: JsonPropertyName("commitTime")] int CommitTime);