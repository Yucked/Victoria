using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victoria.Converters;

internal sealed class LavaTrackConverter : JsonConverter<LavaTrack> {
    public override LavaTrack Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using (JsonDocument trackDocument = JsonDocument.ParseValue(ref reader))
        {
            trackDocument.RootElement.TryGetProperty("encoded", out JsonElement trackHashElement);
            trackDocument.RootElement.TryGetProperty("info", out JsonElement trackElement);
            trackDocument.RootElement.TryGetProperty("pluginInfo", out JsonElement trackPluginInfoElement);

            LavaTrack track = JsonSerializer.Deserialize<LavaTrack>(trackElement);
            track.Hash = trackHashElement.ToString();
            track.PluginInfo = trackPluginInfoElement.ToString();

            return track;
        }
    }

    public override void Write(Utf8JsonWriter writer, LavaTrack value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }
}