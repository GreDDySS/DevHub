using DevHub.Core.Interfaces;
using DevHub.Core.Services;
using DevHub.UI.ViewModels;
using DevHub.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DevHub.UI
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

            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddSingleton<IProjectsScanner, ProjectsScanner>();
            services.AddSingleton<IProjectsService, ProjectsService>();
            services.AddTransient<IIDEOpener, IDEOpener>();


            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<ProjectSettingsViewModel>();
            services.AddTransient<ProjectsViewModel>();
            
            services.AddSingleton<MainWindow>();


            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}
