using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Responses.Rest;

namespace Victoria.Converters {
	internal sealed class RouteResponseConverter : JsonConverter<RouteStatus> {
		public override RouteStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			if (reader.TokenType != JsonTokenType.StartObject) {
				throw new JsonException();
			}

			var routeStatus = new RouteStatus();
			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndObject) {
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName) {
					continue;
				}

				var index = reader.ValueSpan[0];
				reader.Read();

				if (index == 99) {
					routeStatus.Class = reader.GetString();
				}
				else if (index == 100) {
					BuildRouteDetail(ref reader, ref routeStatus);
				}
				else {
					throw new JsonException();
				}
			}

			return routeStatus;
		}

		public override void Write(Utf8JsonWriter writer, RouteStatus value, JsonSerializerOptions options) {
			throw new NotSupportedException("This method cannot be used to write.");
		}

		private static void BuildRouteDetail(ref Utf8JsonReader reader, ref RouteStatus routeStatus) {
			var routeDetail = new RouteDetail();
			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndObject) {
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName) {
					continue;
				}

				if (reader.TryRead("ipBlock")) {
					BuildIPBlock(ref reader, ref routeDetail);
				}
				else if (reader.TryRead("failingAddresses")) {
					BuildFailingAddresses(ref reader, ref routeDetail);
				}
				else if (reader.TryRead("blockIndex")) {
					routeDetail.BlockIndex = reader.GetString();
				}
				else if (reader.TryRead("currentAddressIndex")) {
					routeDetail.CurrentAddressIndex = reader.GetString();
				}
				else if (reader.TryRead("rotateIndex")) {
					routeDetail.RotateIndex = reader.GetString();
				}
				else if (reader.TryRead("ipIndex")) {
					routeDetail.IPIndex = reader.GetString();
				}
				else if (reader.TryRead("currentAddress")) {
					routeDetail.CurrentAddress = reader.GetString();
				}
			}

			routeStatus.Details = routeDetail;
		}

		private static void BuildIPBlock(ref Utf8JsonReader reader, ref RouteDetail routeDetail) {
			var ipBlock = new IPBlock();

			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndObject) {
					break;
				}

				if (reader.TokenType != JsonTokenType.PropertyName) {
					continue;
				}

				var index = reader.ValueSpan[0];
				reader.Read();

				switch (index) {
					case (byte) 't':
						ipBlock.Type = reader.GetString();
						break;
					case (byte) 's':
						ipBlock.Size = reader.GetString();
						break;
				}
			}

			routeDetail.IPBlock = ipBlock;
		}

		private static void BuildFailingAddresses(ref Utf8JsonReader reader, ref RouteDetail routeDetail) {
			var addresses = new List<FailedAddress>();
			while (reader.Read()) {
				if (reader.TokenType == JsonTokenType.EndArray) {
					break;
				}

				if (reader.TokenType != JsonTokenType.StartObject) {
					continue;
				}

				var failedAddress = new FailedAddress();
				while (reader.Read()) {
					if (reader.TokenType == JsonTokenType.EndObject) {
						break;
					}

					if (reader.TokenType != JsonTokenType.PropertyName) {
						continue;
					}

					var index = reader.ValueSpan[^1];
					reader.Read();

					switch (index) {
						case 115:
							failedAddress.Address = reader.GetString();
							break;

						case 112:
							failedAddress.Timestamp = reader.GetInt64();
							break;

						case 101:
							failedAddress.FailedOn = reader.GetString();
							break;
					}
				}

				addresses.Add(failedAddress);
			}

			routeDetail.FailedAddresses = addresses;
		}
	}
}