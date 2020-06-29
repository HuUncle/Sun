using Sun.EventBus.Extensions;

namespace Sun.EventBus.RabbitMQ.Extensions
{
    public static class EventBusOptionExtension
    {
        public static EventBusOption UseRabbitMQ(this EventBusOption option)
        {
            option.RegisterExtension(new RabbitMQOptionExtension());
            return option;
        }
    }
}