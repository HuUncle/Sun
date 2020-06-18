using System;

namespace Sun.Logging
{
    public class LogEntry
    {
        public long Id { get; set; }

        public string ExceptionMessage { get; set; }

        public string Message { get; set; }

        public string Level { get; set; }

        public DateTime TimeStamp { get; set; }

        public string LoggerName { get; set; }
    }
}