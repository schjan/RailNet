using System.Diagnostics;

namespace RailNet.Core.Logging
{
    /// <summary>
    /// Logger welcher alle Lognachrichte an Stdout sendet.
    /// </summary>
    public class DebugLogger : IRailLogger
    {
        public void Log(LogEntry entry)
        {
            Debug.Write($"{entry.Severity.ToString().ToUpper()}: {entry.Message}");
        }
    }
}
