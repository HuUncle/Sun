using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Sun.EventBus.Abstractions;
using Sun.EventBus.RabbitMQ.Extensions;

namespace Sun.EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IOptionsMonitor<RabbitMQOption> _rabbitMQOpions;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _retryCount;
        private readonly ushort _prefetchCount;

        private IModel _consumerChannel;

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection
            , ILogger<EventBusRabbitMQ> logger
            , IOptionsMonitor<RabbitMQOption> rabbitMQOpions
            , IServiceProvider serviceProvider
            , IEventBusSubscriptionsManager subsManager
            , ushort prefetchCount = 1
            , int retryCount = 5)
        {
            _persistentConnection = persistentConnection;
            _logger = logger;
            _rabbitMQOpions = rabbitMQOpions;
            _subsManager = subsManager;
            _serviceProvider = serviceProvider;
            _prefetchCount = prefetchCount;
            _retryCount = retryCount;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        /// <summary>
        /// 推送
        /// </summary>
        /// <param name="event"> </param>
        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex.ToString());
                });

            if (_consumerChannel == null || _consumerChannel.IsClosed)
                _consumerChannel = CreateConsumerChannel();

            _consumerChannel.ExchangeDeclare(exchange: _rabbitMQOpions.CurrentValue.ExchangeName,
                                    type: _rabbitMQOpions.CurrentValue.ExchangeType);

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = _consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                _consumerChannel.BasicPublish(exchange: _rabbitMQOpions.CurrentValue.ExchangeName,
                             routingKey: @event.GetType().Name,
                             mandatory: true,
                             basicProperties: properties,
                             body: body);
            });
        }

        /// <summary>
        /// 订阅注册
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <typeparam name="TH"> </typeparam>
        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);
            _subsManager.AddSubscription<T, TH>();
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <typeparam name="TH"> </typeparam>
        public void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        /// <summary>
        /// 存在未订阅已消费误差,故有之
        /// </summary>
        public void StartSubscribe()
        {
            if (_consumerChannel == null || _consumerChannel.IsClosed)
                _consumerChannel = CreateConsumerChannel();

            _consumerChannel.BasicQos(0, _prefetchCount, false);

            var consumer = new EventingBasicConsumer(_consumerChannel);

            _consumerChannel.BasicConsume(queue: _rabbitMQOpions.CurrentValue.Queue,
                     autoAck: false,
                     consumer: consumer);

            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                await ProcessEvent(eventName, message);

                _consumerChannel.BasicAck(ea.DeliveryTag, multiple: false);
            };

            _consumerChannel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
            };
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            _consumerChannel?.Dispose();

            _subsManager?.Clear();
        }

        /// <summary>
        /// 执行器
        /// </summary>
        /// <param name="eventName"> </param>
        /// <param name="message"> </param>
        /// <returns> </returns>
        private async Task ProcessEvent(string eventName, string message)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        if (eventType != null)
                        {
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType, new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });
                            var handler = scope.ServiceProvider.GetRequiredService(subscription);
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                        }
                    }
                }
            }
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                if (_consumerChannel == null)
                    _consumerChannel = CreateConsumerChannel();

                _consumerChannel.QueueBind(queue: _rabbitMQOpions.CurrentValue.Queue,
                                  exchange: _rabbitMQOpions.CurrentValue.ExchangeName,
                                  routingKey: eventName,
                                  arguments: new Dictionary<string, object> { { "x-queue-mode", "lazy" } });
            }
        }

        /// <summary>
        /// 创建消费监听
        /// </summary>
        /// <returns> </returns>
        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _rabbitMQOpions.CurrentValue.ExchangeName,
                                        type: _rabbitMQOpions.CurrentValue.ExchangeType);

            channel.QueueDeclare(queue: _rabbitMQOpions.CurrentValue.Queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: new Dictionary<string, object> { { "x-queue-mode", "lazy" } });
            return channel;
        }

        /// <summary>
        /// RabbitMQ取消订阅
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="eventName"> </param>
        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _rabbitMQOpions.CurrentValue.Queue,
                    exchange: _rabbitMQOpions.CurrentValue.ExchangeName,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _consumerChannel.Close();
                }
            }
        }
    }
}