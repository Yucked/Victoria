using System.Text.Json.Serialization;

namespace Victoria.Payloads {
	internal abstract class BaseLavaPayload {
		[JsonPropertyName("op")]
		public string Op { get; }

		protected BaseLavaPayload(string op) {
			Op = op;
		}
	}
}