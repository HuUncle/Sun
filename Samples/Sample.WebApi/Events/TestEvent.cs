using System.Threading.Tasks;
using Sun.EventBus;
using Sun.EventBus.Abstractions;

namespace Sample.WebApi.Events
{
    public class TestAEvent : IntegrationEvent
    {
        public TestAEvent(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public class TestAEventHandler : IIntegrationEventHandler<TestAEvent>
    {
        public Task Handle(TestAEvent @event)
        {
            return Task.FromResult(0);
        }
    }

    public class TestBEvent : IntegrationEvent
    {
        public TestBEvent(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public class TestBEventHandler : IIntegrationEventHandler<TestBEvent>
    {
        public Task Handle(TestBEvent @event)
        {
            return Task.FromResult(0);
        }
    }
}