using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Responses.WebSocket;

namespace Victoria.Converters
{
    internal sealed class WebsocketResponseConverter : JsonConverter<BaseWsResponse>
    {
        /// <inheritdoc />
        public override BaseWsResponse Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            return default;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, BaseWsResponse value, JsonSerializerOptions options)
        {
        }
    }
}