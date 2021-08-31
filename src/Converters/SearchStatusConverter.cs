using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Responses.Search;

namespace Victoria.Converters {
    internal sealed class SearchStatusConverter : JsonConverter<SearchStatus> {
        public override SearchStatus Read(ref Utf8JsonReader reader, Type typeToConvert,
                                          JsonSerializerOptions options) {
            return (SearchStatus) reader.ValueSpan[0];
        }

        public override void Write(Utf8JsonWriter writer, SearchStatus value, JsonSerializerOptions options) {
            throw new NotImplementedException();
        }
    }
}