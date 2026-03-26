using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using DevHub.Application.Interfaces;
using DevHub.Application.UseCases.Links;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Interfaces;
using DevHub.Infrastructure.Configuration;
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

        AppPaths.EnsureDirectoriesExist();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: System.IO.Path.Combine(AppPaths.LogsDirectory, "devhub-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        Log.Information("DevHub starting up");

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled exception");

        DispatcherUnhandledException += (_, args) =>
        {
            Log.Error(args.Exception, "Dispatcher unhandled exception");
            args.Handled = true;
        };

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
        var settingsStore = Services.GetRequiredService<JsonSettingsStore>();
        var settings = settingsStore.Load();

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
            Log.Information("DevHub shutting down");
            trayService.Dispose();
            (hotkeyService as IDisposable)?.Dispose();
            Log.CloseAndFlush();
            Shutdown();
        };

        var mainWindow = new MainWindow(mainViewModel, windowService);

        mainWindow.Loaded += (_, _) =>
        {
            if (hotkeyService is WindowsHotkeyService whs)
                whs.Initialize(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle);

            hotkeyService.Register(1, 0x0002 | 0x0004, 0x59);
            Log.Information("Global hotkey registered (Ctrl+Shift+Y)");
        };

        hotkeyService.HotkeyPressed += async _ =>
        {
            Log.Debug("Hotkey pressed, capturing link from clipboard");
            var link = await captureLink.ExecuteAsync();
            if (link is not null)
            {
                Log.Information("Link captured: {Url}", link.Url);
                trayService.ShowBalloon("DevHub", $"Link captured: {link.Url}");
            }
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
        Log.Information("DevHub started successfully");
    }
}
