using NLog;
using RailNet.Core.Logging;

namespace RailNet.Signalsteuerung.WF
{
    public class NLogLogger : IRailLogger
    {
        private readonly Logger _logger;

        public NLogLogger()
        {
            _logger = LogManager.GetLogger("RailNet");
        }

        public void Log(LogEntry entry)
        {
            _logger.Log(LogLevelFromEventType(entry.Severity), entry.Exception, entry.Message);
        }

        private LogLevel LogLevelFromEventType(LoggingEventType eventType)
        {
            switch (eventType)
            {
                case LoggingEventType.Information:
                    return LogLevel.Info;
                case LoggingEventType.Debug:
                    return LogLevel.Debug;
                case LoggingEventType.Error:
                    return LogLevel.Error;
                case LoggingEventType.Fatal:
                    return LogLevel.Fatal;
                case LoggingEventType.Warning:
                    return LogLevel.Warn;
                default:
                    return LogLevel.Trace;
            }
        }
    }
}
