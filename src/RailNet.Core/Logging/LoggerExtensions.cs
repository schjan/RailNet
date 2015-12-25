using System;

namespace RailNet.Core.Logging
{
    internal static class LoggerExtensions
    {
        public static void Trace(this IRailLogger logger, string message, Exception exception = null)
        {
            logger.Log(new LogEntry(LoggingEventType.Trace, message, exception));
        }
        public static void Debug(this IRailLogger logger, string message, Exception exception = null)
        {
            logger.Log(new LogEntry(LoggingEventType.Debug, message, exception));
        }

        public static void Information(this IRailLogger logger, string message)
        {
            logger.Log(new LogEntry(LoggingEventType.Information, message));
        }

        public static void Error(this IRailLogger logger, string message, Exception exception = null)
        {
            logger.Log(new LogEntry(LoggingEventType.Error, message, exception));
        }
    }
}
