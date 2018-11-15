using System;
using Discord;

namespace Victoria.Utilities
{
    internal struct LogResolver
    {
        internal static LogSeverity LogSeverity;

        public static LogMessage Debug(string source, string message, Exception exc = null)
        {
            LogMessage msg = default;
            switch (LogSeverity)
            {
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    msg = Dummy(LogSeverity, source, message, exc);
                    break;
            }

            return msg;
        }

        public static LogMessage Info(string source, string message)
        {
            LogMessage msg = default;
            switch (LogSeverity)
            {
                case LogSeverity.Info:
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    msg = Dummy(LogSeverity, source, message);
                    break;
            }

            return msg;
        }

        public static LogMessage Error(string source, string message, Exception exc = null)
        {
            LogMessage msg = default;
            switch (LogSeverity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                case LogSeverity.Warning:
                case LogSeverity.Error:
                    msg = Dummy(LogSeverity, source, message, exc);
                    break;
            }

            return msg;
        }

        private static LogMessage Dummy(LogSeverity logSeverity, string source, string message, Exception exc = null)
            => new LogMessage(logSeverity, source, message, exc);
    }
}