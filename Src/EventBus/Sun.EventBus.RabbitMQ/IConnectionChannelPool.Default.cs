using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sun.EventBus.RabbitMQ.Extensions;

namespace Sun.EventBus.RabbitMQ
{
    public class ConnectionChannelPoolDefault : IConnectionChannelPool, IDisposable
    {
        private const int DefaultPoolSize = 15;
        private readonly Func<IConnection> _connectionActivator;
        private readonly ILogger<ConnectionChannelPoolDefault> _logger;
        private readonly ConcurrentQueue<IModel> _pool;
        private IConnection _connection;
        private static readonly object sync_root = new object();

        private int _count;
        private int _maxSize;

        public ConnectionChannelPoolDefault(
            ILogger<ConnectionChannelPoolDefault> logger
            , IOptions<RabbitMQOption> rabbitMQOptions)
        {
            _logger = logger;
            _maxSize = DefaultPoolSize;
            _pool = new ConcurrentQueue<IModel>();

            var options = rabbitMQOptions.Value;
            _connectionActivator = CreateConnection(options);

            _logger.LogDebug($"RabbitMQ configuration:'HostName:{options.HostName}, Port:{options.Port}, UserName:{options.UserName}, Password:{options.Password}, ExchangeName:{options.ExchangeName}'");
        }

        IModel IConnectionChannelPool.Rent()
        {
            lock (sync_root)
            {
                while (_count > _maxSize)
                {
                    Thread.SpinWait(1);
                }
                return Rent();
            }
        }

        bool IConnectionChannelPool.Return(IModel connection)
        {
            return Return(connection);
        }

        public IConnection GetConnection()
        {
            if (_connection != null && _connection.IsOpen)
            {
                return _connection;
            }

            _connection = _connectionActivator();
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            return _connection;
        }

        private static Func<IConnection> CreateConnection(RabbitMQOption option)
        {
            var serviceName = Assembly.GetEntryAssembly()?.GetName().Name.ToLower();

            var factory = new ConnectionFactory
            {
                UserName = option.UserName,
                Port = option.Port,
                Password = option.Password,
                VirtualHost = option.VirtualHost
            };

            if (option.HostName.Contains(","))
            {
                option.ConnectionFactoryOption?.Invoke(factory);
                return () => factory.CreateConnection(
                    option.HostName.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries), serviceName);
            }
            factory.HostName = option.HostName;
            option.ConnectionFactoryOption?.Invoke(factory);
            return () => factory.CreateConnection(serviceName);
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning($"RabbitMQ client connection closed! --> {e.ReplyText}");
        }

        public virtual IModel Rent()
        {
            if (_pool.TryDequeue(out var model))
            {
                Interlocked.Decrement(ref _count);

                Debug.Assert(_count >= 0);

                return model;
            }

            try
            {
                model = GetConnection().CreateModel();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "RabbitMQ channel model create failed!");
                Console.WriteLine(e);
                throw;
            }

            return model;
        }

        public virtual bool Return(IModel connection)
        {
            if (Interlocked.Increment(ref _count) <= _maxSize && connection.IsOpen)
            {
                _pool.Enqueue(connection);

                return true;
            }

            Interlocked.Decrement(ref _count);

            Debug.Assert(_maxSize == 0 || _pool.Count <= _maxSize);

            return false;
        }

        public void Dispose()
        {
            _maxSize = 0;

            while (_pool.TryDequeue(out var context))
            {
                context.Dispose();
            }
        }
    }
}