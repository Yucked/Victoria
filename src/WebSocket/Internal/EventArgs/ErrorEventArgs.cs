using System;

namespace Victoria.WebSocket.Internal.EventArgs;

/// <summary>
/// 
/// </summary>
public readonly record struct ErrorEventArgs(Exception Exception, string Message) {
    internal ErrorEventArgs(Exception exception) : this(exception, exception.Message) { }
    internal ErrorEventArgs(string message) : this(default, message) { }
}