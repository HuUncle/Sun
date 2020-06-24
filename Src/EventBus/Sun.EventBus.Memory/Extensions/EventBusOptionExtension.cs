using Sun.EventBus.Extensions;

namespace Sun.EventBus.Memory.Extensions
{
    public static class EventBusOptionExtension
    {
        public static void UseMemory(this EventBusOption option)
        {
            option.RegisterExtension(new EventBusMemoryOption());
        }
    }
}