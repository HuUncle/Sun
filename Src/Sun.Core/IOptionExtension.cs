using Microsoft.Extensions.DependencyInjection;

namespace Sun.Core
{
    public interface IOptionExtension
    {
        void AddServices(IServiceCollection services);
    }
}