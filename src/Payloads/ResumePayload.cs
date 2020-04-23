using System;
using System.Text.Json.Serialization;

namespace Victoria.Payloads {
	internal sealed class ResumePayload : BaseLavaPayload {
		[JsonPropertyName("key")]
		public string Key { get; }

		[JsonPropertyName("timeout")]
		public long Timeout { get; }

		public ResumePayload(string key, TimeSpan timeout) : base("configureResuming") {
			Key = key;
			Timeout = (long) timeout.TotalSeconds;
		}
	}
}