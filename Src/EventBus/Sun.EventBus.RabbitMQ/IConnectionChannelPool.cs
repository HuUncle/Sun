using RabbitMQ.Client;

namespace Sun.EventBus.RabbitMQ
{
    public interface IConnectionChannelPool
    {
        IConnection GetConnection();

        IModel Rent();

        bool Return(IModel context);
    }
}