using System;
using Microsoft.Extensions.DependencyInjection;

namespace Sun.EventBus.Extensions
{
    public static class EventBusExtension
    {
        public static void AddEventBus(this IServiceCollection services, Action<EventBusOption> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var options = new EventBusOption();
            action.Invoke(options);

            services.AddSingleton<IEventBusSubscriptionsManager, EventBusSubscriptionsManagerDefault>();

            foreach (var item in options.Extensions)
                item.AddServices(services);
        }
    }
}