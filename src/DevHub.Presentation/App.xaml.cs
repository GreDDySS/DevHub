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

namespace DevHub.Presentation;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
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

            // Infrastructure services
            services.AddSingleton<IProcessLauncher, ProcessLauncher>();
            services.AddSingleton<IClipboardService, Services.ClipboardService>();
            services.AddSingleton<IHotkeyService, WindowsHotkeyService>();
            services.AddSingleton<IdeScanner>();
            services.AddSingleton<IAppSettingsStore, JsonSettingsStore>();
            services.AddSingleton<TrayService>();
            services.AddSingleton<IAutostartService, AutostartService>();

            // Use Cases
            services.AddSingleton<GetAllProjectsUseCase>();
            services.AddSingleton<AddProjectUseCase>();
            services.AddSingleton<UpdateProjectUseCase>();
            services.AddSingleton<CaptureLinkUseCase>();

            // Presentation services
            var registry = new ViewRegistry();
            services.AddSingleton(registry);
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton(sp => (WindowService)sp.GetRequiredService<IWindowService>());

            // ViewModels — регистрируются только через ViewFactoryService

            // Автоматическая регистрация ViewModels и Views
            var factory = new ViewFactoryService(registry);
            factory.RegisterAll(services, typeof(App).Assembly);

            // Один BuildServiceProvider
            Services = services.BuildServiceProvider();
            factory.SetServiceProvider(Services);

            Log.Debug("Resolving services...");
            var windowService = Services.GetRequiredService<WindowService>();
            var mainViewModel = Services.GetRequiredService<ViewModels.MainViewModel>();
            var trayService = Services.GetRequiredService<TrayService>();
            var hotkeyService = Services.GetRequiredService<IHotkeyService>();
            var captureLink = Services.GetRequiredService<CaptureLinkUseCase>();
            var settingsStore = Services.GetRequiredService<IAppSettingsStore>();

            Log.Debug("Loading settings...");
            var settings = await settingsStore.LoadAsync();

            Log.Debug("Initializing tray service...");
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

            Log.Debug("Creating MainWindow...");
            var mainWindow = new MainWindow(mainViewModel, windowService);
            MainWindow = mainWindow;

            mainWindow.Loaded += (_, _) =>
            {
                Log.Debug("MainWindow Loaded event fired");
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

            mainWindow.Closing += async (s, args) =>
            {
                var action = settings.CloseAction;

                if (action == Domain.Enums.CloseAction.Ask)
                {
                    var dialog = new Views.CloseDialogView { Owner = mainWindow };
                    if (dialog.ShowDialog() == true)
                    {
                        if (dialog.ShouldRemember)
                        {
                            settings.CloseAction = dialog.MinimizeToTray
                                ? Domain.Enums.CloseAction.MinimizeToTray
                                : Domain.Enums.CloseAction.Exit;
                            await settingsStore.SaveAsync(settings);
                        }

                        if (dialog.MinimizeToTray)
                        {
                            args.Cancel = true;
                            windowService.MinimizeToTray();
                            trayService.Show();
                        }
                    }
                    else
                    {
                        args.Cancel = true;
                    }
                }
                else if (action == Domain.Enums.CloseAction.MinimizeToTray)
                {
                    args.Cancel = true;
                    windowService.MinimizeToTray();
                    trayService.Show();
                }
            };

            Log.Debug("Calling MainWindow.Show()...");
            mainWindow.Show();
            Log.Information("DevHub started successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to start DevHub");
            System.Windows.MessageBox.Show($"Failed to start DevHub: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
