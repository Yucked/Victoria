using System.Text.Json.Serialization;

namespace Victoria.Responses.Rest {
	internal struct RouteResponse {
		[JsonPropertyName("error")]
		public string Error { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; }
	}
}