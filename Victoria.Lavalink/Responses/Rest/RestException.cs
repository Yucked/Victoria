using System.Text.Json.Serialization;

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
        [JsonPropertyName("message")]
        public string Message { get; }

        /// <summary>
        ///     Severity represents how common the error is.
        ///     A severity level of COMMON indicates that the error is non-fatal and that the issue is not from Lavalink itself.
        /// </summary>
        [JsonPropertyName("severity")]
        public string Severity { get; }

        internal RestException(string message, string severity)
        {
            Message = message;
            Severity = severity;
        }
    }
}