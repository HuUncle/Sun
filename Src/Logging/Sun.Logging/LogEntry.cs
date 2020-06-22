using System;

namespace Sun.Logging
{
    public class LogEntry
    {
        public LogEntry()
        {
            TimeStamp = DateTime.Now;
        }

        public long Id { get; set; }

        public string Message { get; set; }

        public string Level { get; set; }

        public Exception Exception { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}