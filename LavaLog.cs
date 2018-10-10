using System;
using Discord;

namespace Victoria
{
    public sealed class LavaLog
    {
        internal LavaLog()
        {
        }

        internal LavaLog(LogSeverity severity, string message, Exception exception)
        {
            Severity = severity;
            Message = message;
            Exception = exception;
        }

        public string Message { get; }
        public Exception Exception { get; }
        public LogSeverity Severity { get; }
    }
}