using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Sun.EventBus.RabbitMQ.Extensions;

namespace Sun.EventBus.RabbitMQ
{
    public class DefaultRabbitMQPersistentConnection
       : IRabbitMQPersistentConnection
    {
        private readonly IOptionsMonitor<RabbitMQOption> _rabbitMQOptions;
        private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
        private readonly int _retryCount;
        private IConnection _connection;
        private bool _disposed;

        private object sync_root = new object();

        public DefaultRabbitMQPersistentConnection(
              IOptionsMonitor<RabbitMQOption> rabbitMQOptions
            , ILogger<DefaultRabbitMQPersistentConnection> logger
            , int retryCount = 5)
        {
            _rabbitMQOptions = rabbitMQOptions;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryCount = retryCount;
        }

        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (IsConnected)
            {
                if (_disposed) return;

                _disposed = true;

                try
                {
                    _connection.Dispose();
                }
                catch (IOException ex)
                {
                    _logger.LogCritical(ex.ToString());
                }
            }
        }

        public bool TryConnect()
        {
            lock (sync_root)
            {
                try
                {
                    var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_retryCount
                        , retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        , (ex, time) =>
                        {
                            _logger.LogWarning(ex.ToString());
                        });

                    policy.Execute(() =>
                    {
                        var factory = new ConnectionFactory
                        {
                            UserName = _rabbitMQOptions.CurrentValue.UserName,
                            Port = _rabbitMQOptions.CurrentValue.Port,
                            Password = _rabbitMQOptions.CurrentValue.Password,
                            VirtualHost = _rabbitMQOptions.CurrentValue.VirtualHost
                        };

                        if (_rabbitMQOptions.CurrentValue.HostName.Contains(","))
                        {
                            var serviceName = Assembly.GetEntryAssembly()?.GetName().Name.ToLower();

                            _rabbitMQOptions.CurrentValue.ConnectionFactoryOption?.Invoke(factory);

                            _connection = factory.CreateConnection(
                                _rabbitMQOptions.CurrentValue.HostName.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), serviceName);
                        }
                        else
                        {
                            factory.HostName = _rabbitMQOptions.CurrentValue.HostName;
                            _connection = factory.CreateConnection();
                        }
                    });

                    if (IsConnected)
                    {
                        _connection.ConnectionShutdown += OnConnectionShutdown;
                        _connection.CallbackException += OnCallbackException;
                        _connection.ConnectionBlocked += OnConnectionBlocked;

                        _logger.LogInformation($"RabbitMQ persistent connection acquired a connection { _connection.Endpoint.HostName} and is subscribed to failure events");

                        return true;
                    }
                    else
                    {
                        _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
    }
}