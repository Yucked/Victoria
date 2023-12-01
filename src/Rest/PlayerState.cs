using System;
using System.Text.Json.Serialization;
using Victoria.Converters;

namespace Victoria.Rest;

/// <summary>
/// 
/// </summary>
/// <param name="Time"></param>
/// <param name="Position"></param>
/// <param name="IsConnected"></param>
/// <param name="Ping"></param>
public record struct PlayerState(
    [property: JsonPropertyName("time"), JsonConverter(typeof(TimeSpanConverter))]
    TimeSpan Time,
    [property: JsonPropertyName("position"), JsonConverter(typeof(TimeSpanConverter))]
    TimeSpan Position,
    [property: JsonPropertyName("connected")]
    bool IsConnected,
    [property: JsonPropertyName("ping")]
    int Ping);