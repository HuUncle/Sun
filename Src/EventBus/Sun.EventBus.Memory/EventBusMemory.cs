using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sun.EventBus.Abstractions;

namespace Sun.EventBus.Memory
{
    public class EventBusMemory : IEventBus
    {
        private readonly IEventBusSubscriptionsManager _subManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentQueue<IntegrationEvent> _eventsQueue;

        public EventBusMemory(IEventBusSubscriptionsManager subscriptionsManager, IServiceProvider serviceProvider)
        {
            _subManager = subscriptionsManager;
            _serviceProvider = serviceProvider;
            _eventsQueue = new ConcurrentQueue<IntegrationEvent>();
        }

        public void Publish(IntegrationEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            _eventsQueue.Enqueue(@event);
        }

        public void StartSubscribe()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (_eventsQueue.TryDequeue(out var @event))
                    {
                        await ProcessEvent(@event);
                    }
                }
            });
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _subManager.AddSubscription<T, TH>();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _subManager.RemoveSubscription<T, TH>();
        }

        private async Task ProcessEvent(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            if (_subManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _subManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        var eventType = _subManager.GetEventTypeByName(eventName);
                        if (eventType != null)
                        {
                            var instance = scope.ServiceProvider.GetRequiredService(subscription);
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(instance, new object[] { @event });
                        }
                    }
                }
            }
        }
    }
}