using DevHub.Core.Attributes;
using DevHub.Core.Interfaces;
using DevHub.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevHub.Infrastructure
{
    [Module("Infrastructure", isSystem: true)]
    public class InfrastructureModule : IModule
    {
        public void RegisterTypes(IServiceCollection services)
        {
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IProjectsScanner, ProjectsScanner>();
            services.AddTransient<IIDEOpener, IDEOpener>();
        }

        public void OnInitialized(IServiceProvider provider)
        {
        }
    }
}
