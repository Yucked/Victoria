using System;
using System.IO;
using System.Text.Json;

namespace Victoria.Rest;

/// <summary>
/// When Lavalink encounters an error, it will respond with a JSON object containing more information about the error.
/// Include the trace=true query param to also receive the full stack trace.
/// </summary>
public class RestException : Exception {
    /// <summary>
    /// The timestamp of the error in milliseconds since the epoch
    /// </summary>
    public int Timestamp { get; }

    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; }

    /// <summary>
    /// The HTTP status code message
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// The stack trace of the error when trace=true as query param has been sent
    /// </summary>
    public string Trace { get; }

    /// <summary>
    /// Error message
    /// </summary>
    public override string Message { get; }

    /// <summary>
    /// The request path
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    private RestException(Stream stream) {
        var root = JsonDocument.Parse(stream).RootElement;
        var timestamp = root.GetProperty("timestamp").GetUInt64();
        Status = root.GetProperty("status").GetInt32();
        Error = root.GetProperty("error").GetString();

        //Trace = root.GetProperty("trace").GetString();
        Message = root.GetProperty("message").GetString()!;
        Path = root.GetProperty("path").GetString();
    }

    /// <inheritdoc />
    public override string ToString() {
        return $"{Error}: {Message}\n{Path}\n{Status}\n{DateTimeOffset.FromUnixTimeMilliseconds(Timestamp)}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isSuccessStatusCode"></param>
    /// <param name="stream"></param>
    /// <exception cref="RestException"></exception>
    public static void ThrowIfNot200(bool isSuccessStatusCode, Stream stream) {
        if (isSuccessStatusCode) {
            return;
        }

        throw new RestException(stream);
    }
}