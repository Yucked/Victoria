using System.Text.Json.Serialization;

namespace Victoria.Rest;

/// <summary>
/// The Json object returned by the API when calling the Track Decoding endpoints
/// </summary>
public class DecodedLavaTrack
{
    /// <summary>
    /// The encoded track data
    /// </summary>
    [JsonPropertyName("encoded")]
    public string Encoded { get; set; }

    /// <summary>
    /// The decoded LavaTrack
    /// </summary>
    [JsonPropertyName("info")]
    public LavaTrack Info { get; set; }

    /// <summary>
    /// Additional track info provided by plugins
    /// </summary>
    [JsonPropertyName("pluginInfo")]
    public object PluginInfo { get; set; }

    /// <summary>
    /// Additional track data provided via the Update Player endpoint
    /// </summary>
    [JsonPropertyName("userData")]
    public object UserData { get; set; }
}