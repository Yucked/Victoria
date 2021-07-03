using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Victoria.Converters {
    internal sealed class LongToTimeSpanConverter : JsonConverter<TimeSpan> {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var duration = reader.GetInt64();
            return duration < TimeSpan.MaxValue.Ticks
                ? TimeSpan.FromMilliseconds(duration)
                : TimeSpan.MaxValue;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) {
            writer.WriteNumberValue(value.TotalMilliseconds);
        }
    }
}