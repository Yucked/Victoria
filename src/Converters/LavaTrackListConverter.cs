using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victoria.Converters;

internal sealed class LavaTrackListConverter : JsonConverter<IReadOnlyCollection<LavaTrack>> {
    public override IReadOnlyCollection<LavaTrack> Read(ref Utf8JsonReader reader,
                                                        Type typeToConvert,
                                                        JsonSerializerOptions options) {
        using var trackDocument = JsonDocument.ParseValue(ref reader);
        return trackDocument.RootElement
            .EnumerateArray()
            .Select(element => element.Deserialize<LavaTrack>(Extensions.Options))
            .ToList();
    }
    
    public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<LavaTrack> value,
                               JsonSerializerOptions options) {
        throw new NotImplementedException();
    }
}