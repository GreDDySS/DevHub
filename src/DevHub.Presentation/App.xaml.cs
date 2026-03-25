using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DevHub.Application.Interfaces;
using DevHub.Presentation.Registry;
using DevHub.Presentation.Services;
using DevHub.Presentation.ViewModels;

namespace DevHub.Presentation;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        var registry = new ViewRegistry();
        services.AddSingleton(registry);

        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton(sp => (WindowService)sp.GetRequiredService<IWindowService>());

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ProjectListViewModel>();
        services.AddSingleton<SettingsViewModel>();

        Services = services.BuildServiceProvider();

        var factory = new ViewFactoryService(registry, Services);
        factory.RegisterAll(services, typeof(App).Assembly);

        Services = services.BuildServiceProvider();

        var windowService = Services.GetRequiredService<WindowService>();
        var mainViewModel = Services.GetRequiredService<MainViewModel>();

        var mainWindow = new MainWindow(mainViewModel, windowService);
        mainWindow.Show();
    }
}
