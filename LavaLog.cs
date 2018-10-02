using System;
using Discord;

namespace Victoria
{
    public sealed class LavaLog
    {
        public string Message { get; }
        public Exception Exception { get; }
        public LogSeverity Severity { get; }

        internal LavaLog()
        {
        }

        internal LavaLog(LogSeverity severity, string message, Exception exception)
        {
            Severity = severity;
            Message = message;
            Exception = exception;
        }
    }
}