using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Enums;

namespace Victoria.Converters {
    internal sealed class LoadStatusConverter : JsonConverter<LoadStatus> {
        public override LoadStatus Read(ref Utf8JsonReader reader, Type typeToConvert,
                                          JsonSerializerOptions options) {
            return (LoadStatus)reader.ValueSpan[0];
        }

        public override void Write(Utf8JsonWriter writer, LoadStatus value, JsonSerializerOptions options) {
            throw new NotImplementedException();
        }
    }
}