using Microsoft.Extensions.DependencyInjection;
using Sun.Core;
using Sun.EventBus.Abstractions;

namespace Sun.EventBus.RabbitMQ.Extensions
{
    public class RabbitMQOptionExtension : IOptionExtension
    {
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>();
            services.AddSingleton<IEventBus, EventBusRabbitMQ>();
        }
    }
}