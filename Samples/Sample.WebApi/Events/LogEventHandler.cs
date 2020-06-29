using System.Threading.Tasks;
using Sun.EventBus.Abstractions;
using Sun.Logging.EventBusStore;

namespace Sample.WebApi.Events
{
    public class LogEventHandler : IIntegrationEventHandler<LogEvent>
    {
        public Task Handle(LogEvent @event)
        {
            return Task.FromResult(0);
        }
    }
}