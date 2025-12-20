using DevHub.Core.Interfaces;
using DevHub.Infrastructure;
using DevHub.Infrastructure.Services;
using DevHub.Modules.Projects;
using DevHub.Shell.ViewModels;
using DevHub.Shell.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DevHub.Shell
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; }

        public App()
        {
            Services = ConfigureService();
        }

        private static IServiceProvider ConfigureService()
        {
            var services = new ServiceCollection();


            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<ProjectSettingsViewModel>();
            
            services.AddSingleton<MainWindow>();

            var loader = new ModuleLoader(AppDomain.CurrentDomain.BaseDirectory);
            loader.LoadModules(services);

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var modules = Services.GetServices<IModule>();
            foreach (var module in modules)
            {
                module.OnInitialized(Services);
            }

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}
