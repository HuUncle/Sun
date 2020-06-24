using Microsoft.Extensions.DependencyInjection;
using Sun.Core;
using Sun.EventBus.Abstractions;

namespace Sun.EventBus.Memory.Extensions
{
    public class EventBusMemoryOption : IOptionExtension
    {
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IEventBus, EventBusMemory>();
        }
    }
}