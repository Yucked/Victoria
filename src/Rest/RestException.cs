using System;

namespace Victoria.Rest;

/// <summary>
/// When Lavalink encounters an error, it will respond with a JSON object containing more information about the error.
/// Include the trace=true query param to also receive the full stack trace.
/// </summary>
public class RestException : Exception {
    /// <summary>
    /// The timestamp of the error in milliseconds since the epoch
    /// </summary>
    public int Timestamp { get; init; }
    
    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; init; }
    
    /// <summary>
    /// The HTTP status code message
    /// </summary>
    public string Error { get; init; }
    
    /// <summary>
    /// The stack trace of the error when trace=true as query param has been sent
    /// </summary>
    public string Trace { get; init; }
    
    /// <summary>
    /// The request path
    /// </summary>
    public string Path { get; init; }
}