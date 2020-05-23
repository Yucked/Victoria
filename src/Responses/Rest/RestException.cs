namespace Victoria.Responses.Rest {
	/// <summary>
	///     If LoadStatus was LoadFailed then Exception is returned.
	/// </summary>
	public struct RestException {
		/// <summary>
		///     Details why the track failed to load.
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		///     Severity represents how common the error is.
		///     A severity level of COMMON indicates that the error is non-fatal and that the issue is not from Lavalink itself.
		/// </summary>
		public string Severity { get; private set; }

		internal void WithMessage(string message) {
			Message = message;
		}

		internal void WithSeverity(string severity) {
			Severity = severity;
		}
	}
}