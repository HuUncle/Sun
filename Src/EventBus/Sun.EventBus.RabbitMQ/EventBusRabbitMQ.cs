using System;
using System.Collections.Generic;
using System.Text;
using Sun.EventBus.Abstractions;

namespace Sun.EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private const string BROKER_NAME = "sun_event_bus";

        private readonly IConnectionChannelPool _connectionChannelPool;
        private readonly IEventBusSubscriptionsManager _subManager;

        private
             string _queueName = "";

        public EventBusRabbitMQ(
            IConnectionChannelPool connectionChannelPool
          , IEventBusSubscriptionsManager subManager)
        {
            _connectionChannelPool = connectionChannelPool;
            _subManager = subManager;
        }

        public void Publish(IntegrationEvent @event)
        {
        }

        public void StartSubscribe()
        {
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subManager.GetEventKey<T>();
            //DoInternalSubscription(eventName);
            _subManager.AddSubscription<T, TH>();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _subManager.RemoveSubscription<T, TH>();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                //if (!_persistentConnection.IsConnected)
                //{
                //    _persistentConnection.TryConnect();
                //}

                //if (_consumerChannel == null)
                //    _consumerChannel = CreateConsumerChannel();

                //using (var channel = _persistentConnection.CreateModel())
                //{
                //    channel.QueueBind(queue: _queueName,
                //                      exchange: BROKER_NAME,
                //                      routingKey: eventName,
                //                      arguments: new Dictionary<string, object> { { "x-queue-mode", "lazy" } });
                //}
            }
        }

        public void Dispose()
        {
        }
    }
}