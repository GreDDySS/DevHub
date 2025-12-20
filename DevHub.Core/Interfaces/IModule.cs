using Microsoft.Extensions.DependencyInjection;

namespace DevHub.Core.Interfaces
{
    public interface IModule
    {
        void RegisterTypes(IServiceCollection services);
    }
}
