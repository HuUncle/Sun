using Sun.EventBus;

namespace Sun.Logging.EventBusStore
{
    public class LogEvent : IntegrationEvent
    {
        public LogEvent(LogEntry logInfo)
        {
            LogInfo = logInfo;
        }

        public LogEntry LogInfo { get; set; }
    }
}