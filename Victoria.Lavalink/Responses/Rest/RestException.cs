using Newtonsoft.Json;

namespace Victoria.Lavalink.Responses.Rest
{
    /// <summary>
    ///     If LoadType was LoadFailed then Exception is returned.
    /// </summary>
    public readonly struct RestException
    {
        /// <summary>
        ///     Details why the track failed to load.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; }

        /// <summary>
        ///     Severity represents how common the error is.
        ///     A severity level of COMMON indicates that the error is non-fatal and that the issue is not from Lavalink itself.
        /// </summary>
        [JsonProperty("severity")]
        public string Severity { get; }

        internal RestException(string message, string severity)
        {
            Message = message;
            Severity = severity;
        }
    }
}
