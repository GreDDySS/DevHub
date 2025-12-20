using DevHub.Core.Attributes;
using DevHub.Core.Interfaces;
using DevHub.Modules.Projects.Controls;
using DevHub.Modules.Projects.Services;
using DevHub.Modules.Projects.ViewModels;
using DevHub.Modules.Projects.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DevHub.Modules.Projects
{
    [Module("Projects", isSystem: true)]
    public class ProjectsModule : IModule
    {
        public void RegisterTypes(IServiceCollection services)
        {
            services.AddSingleton<IProjectsService, ProjectsService>();

            services.AddSingleton<ProjectsViewModel>();
            services.AddSingleton<ProjectCardControl>();

            services.AddTransient<ProjectsView>();
        }

        public void OnInitialized(IServiceProvider provider)
        {

        }
    }
}
