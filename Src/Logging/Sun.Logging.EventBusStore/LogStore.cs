using Sun.EventBus.Abstractions;

namespace Sun.Logging.EventBusStore
{
    public class LogStore : ILogStore
    {
        private readonly IEventBus _eventBus;

        public LogStore(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Post(LogEntry entry)
        {
            _eventBus.Publish(new LogEvent(entry));
        }
    }
}