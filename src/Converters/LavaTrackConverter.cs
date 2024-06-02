using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victoria.Converters;

internal sealed class LavaTrackConverter : JsonConverter<LavaTrack> {
    public override LavaTrack Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using var trackDocument = JsonDocument.ParseValue(ref reader);
        
        trackDocument.RootElement.TryGetProperty("encoded", out var trackHashElement);
        trackDocument.RootElement.TryGetProperty("info", out var trackElement);
        trackDocument.RootElement.TryGetProperty("pluginInfo", out var trackPluginInfoElement);
        
        var track = trackElement.Deserialize<LavaTrack>();
        track.Hash = trackHashElement.ToString();
        track.PluginInfo = trackPluginInfoElement.ToString();
        
        return track;
    }
    
    public override void Write(Utf8JsonWriter writer, LavaTrack value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }
}