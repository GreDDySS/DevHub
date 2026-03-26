using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DevHub.Application.Interfaces;
using DevHub.Application.UseCases.Links;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Interfaces;
using DevHub.Infrastructure.Services;
using DevHub.Infrastructure.Storage;
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

        // Repositories
        services.AddSingleton<IProjectRepository, JsonProjectRepository>();
        services.AddSingleton<ILinkRepository, JsonLinkRepository>();

        // Services
        services.AddSingleton<IProcessLauncher, ProcessLauncher>();
        services.AddSingleton<IClipboardService, Presentation.Services.ClipboardService>();
        services.AddSingleton<IHotkeyService, WindowsHotkeyService>();
        services.AddSingleton<JsonSettingsStore>();
        services.AddSingleton<TrayService>();
        services.AddSingleton<AutostartService>();

        // Use Cases
        services.AddSingleton<GetAllProjectsUseCase>();
        services.AddSingleton<AddProjectUseCase>();
        services.AddSingleton<UpdateProjectUseCase>();
        services.AddSingleton<CaptureLinkUseCase>();

        // Registry & Services
        var registry = new ViewRegistry();
        services.AddSingleton(registry);
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton(sp => (WindowService)sp.GetRequiredService<IWindowService>());

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ProjectListViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<LinkListViewModel>();
        services.AddTransient<AddProjectViewModel>();

        Services = services.BuildServiceProvider();

        var factory = new ViewFactoryService(registry, Services);
        factory.RegisterAll(services, typeof(App).Assembly);

        Services = services.BuildServiceProvider();

        var windowService = Services.GetRequiredService<WindowService>();
        var mainViewModel = Services.GetRequiredService<MainViewModel>();
        var trayService = Services.GetRequiredService<TrayService>();
        var hotkeyService = Services.GetRequiredService<IHotkeyService>();
        var captureLink = Services.GetRequiredService<CaptureLinkUseCase>();
        var settings = Services.GetRequiredService<JsonSettingsStore>().Load();

        trayService.Initialize();

        trayService.ShowWindowRequested += () =>
        {
            windowService.RestoreFromTray();
            trayService.Hide();
        };

        trayService.TrayDoubleClick += () =>
        {
            windowService.RestoreFromTray();
            trayService.Hide();
        };

        trayService.ExitRequested += () =>
        {
            trayService.Dispose();
            (hotkeyService as IDisposable)?.Dispose();
            Shutdown();
        };

        var mainWindow = new MainWindow(mainViewModel, windowService);

        mainWindow.Loaded += (_, _) =>
        {
            if (hotkeyService is WindowsHotkeyService whs)
                whs.Initialize(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle);

            hotkeyService.Register(1, 0x0002 | 0x0004, 0x59);
        };

        hotkeyService.HotkeyPressed += async _ =>
        {
            var link = await captureLink.ExecuteAsync();
            if (link is not null)
                trayService.ShowBalloon("DevHub", $"Link captured: {link.Url}");
        };

        mainWindow.Closing += (s, args) =>
        {
            if (settings.MinimizeToTray)
            {
                args.Cancel = true;
                windowService.MinimizeToTray();
                trayService.Show();
            }
        };

        mainWindow.Show();
    }
}
