namespace RailNet.Core.Logging
{
    /// <summary>
    /// Logger which logs nothing.
    /// </summary>
    internal class NullLogger : IRailLogger
    {
        public void Log(LogEntry entry)
        {
            //Do nothing.
        }
    }
}
