namespace Victoria.Responses.Rest {
	/// <summary>
	///     Contains information about route planner class and details.
	/// </summary>
	public struct RouteStatus {
		/// <summary>
		///     Which planner class is being used.
		/// </summary>
		public string Class { get; internal set; }

		/// <summary>
		///     Gives more information about route planner.
		/// </summary>
		public RouteDetail Details { get; internal set; }
	}
}