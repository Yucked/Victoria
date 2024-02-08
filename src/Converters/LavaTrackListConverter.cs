using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victoria.Converters;

internal sealed class LavaTrackListConverter : JsonConverter<IReadOnlyCollection<LavaTrack>>
{
    public override IReadOnlyCollection<LavaTrack> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument trackDocument = JsonDocument.ParseValue(ref reader))
        {
            List<LavaTrack> trackList = new List<LavaTrack>();
            foreach (JsonElement element in trackDocument.RootElement.EnumerateArray())
            {
                trackList.Add(JsonSerializer.Deserialize<LavaTrack>(element, Extensions.Options));
            }

            return new ReadOnlyCollection<LavaTrack>(trackList);
        }
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<LavaTrack> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}