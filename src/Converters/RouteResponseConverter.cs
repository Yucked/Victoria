using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Responses.Rest;

namespace Victoria.Converters {
	internal sealed class RouteResponseConverter : JsonConverter<RouteStatus> {
		public override RouteStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, RouteStatus value, JsonSerializerOptions options) {
			throw new NotImplementedException();
		}
	}
}