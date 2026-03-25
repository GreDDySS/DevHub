using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using DevHub.Application.Interfaces;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;
using DevHub.Presentation.Registry;

namespace DevHub.Presentation.Services;

public class WindowService : IWindowService
{
    private readonly ViewRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, object> _singletonCache = new();
    private ContentControl? _navigationHost;

    public WindowService(ViewRegistry registry, IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    public void SetNavigationHost(ContentControl host)
    {
        _navigationHost = host;
    }

    public void NavigateTo(string key)
    {
        var registration = _registry.GetByKey(key);
        if (registration is null)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation view '{key}' not found");
            return;
        }

        var view = ResolveView(registration);
        _navigationHost!.Content = view;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var registration = _registry.GetByViewModel<TViewModel>();
        if (registration is null)
        {
            System.Diagnostics.Debug.WriteLine($"View for {typeof(TViewModel).Name} not found");
            return;
        }

        var view = ResolveView(registration);
        _navigationHost!.Content = view;
    }

    public object? GetCurrentView()
        => _navigationHost?.Content;

    public bool? ShowDialog(Type viewModelType)
    {
        var viewModel = _serviceProvider.GetRequiredService(viewModelType) as ViewModelBase;
        return ShowDialogInternal(viewModel!);
    }

    public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        => ShowDialogInternal(viewModel);

    public void Show(Type viewModelType)
    {
        var viewModel = _serviceProvider.GetRequiredService(viewModelType) as ViewModelBase;
        ShowInternal(viewModel!);
    }

    public void Show<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        => ShowInternal(viewModel);

    public bool Confirm(string title, string message)
    {
        return MessageBox.Show(message, title,
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public string? OpenFolderDialog(string initialDirectory = "")
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select folder",
            InitialDirectory = initialDirectory
        };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public string? OpenFileDialog(string filter = "All files (*.*)|*.*", string initialDirectory = "")
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = filter,
            InitialDirectory = initialDirectory
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void ShowNotification(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void CloseWindow(object viewModel)
    {
        System.Windows.Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.DataContext == viewModel)
            ?.Close();
    }

    public void MinimizeToTray()
    {
        if (System.Windows.Application.Current.MainWindow is { } main)
            main.Hide();
    }

    public void RestoreFromTray()
    {
        if (System.Windows.Application.Current.MainWindow is { } main)
        {
            main.Show();
            main.WindowState = WindowState.Normal;
            main.Activate();
        }
    }

    private bool? ShowDialogInternal(ViewModelBase viewModel)
    {
        var window = CreateWindow(viewModel);
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.Owner = System.Windows.Application.Current.MainWindow;
        return window.ShowDialog();
    }

    private void ShowInternal(ViewModelBase viewModel)
    {
        var window = CreateWindow(viewModel);
        window.Show();
    }

    private Window CreateWindow(ViewModelBase viewModel)
    {
        var registration = _registry.GetByViewModel(viewModel.GetType())
            ?? throw new InvalidOperationException($"View for {viewModel.GetType().Name} not registered");

        var window = (Window)Activator.CreateInstance(registration.ViewType)!;
        window.DataContext = viewModel;

        if (viewModel is BaseWindowViewModel vm)
        {
            window.Loaded += (_, _) => vm.OnWindowLoaded();
            window.Closing += (_, e) => vm.OnWindowClosing(e);
            window.Closed += (_, _) => vm.OnWindowClosed();
        }

        return window;
    }

    private FrameworkElement ResolveView(ViewRegistration registration)
    {
        var viewModel = ResolveViewModel(registration);
        var view = (FrameworkElement)Activator.CreateInstance(registration.ViewType)!;
        view.DataContext = viewModel;
        return view;
    }

    private ViewModelBase ResolveViewModel(ViewRegistration registration)
    {
        if (registration.IsSingleton)
        {
            if (_singletonCache.TryGetValue(registration.ViewModelType, out var cached))
                return (ViewModelBase)cached;

            var vm = (ViewModelBase)_serviceProvider.GetRequiredService(registration.ViewModelType);
            _singletonCache[registration.ViewModelType] = vm;
            return vm;
        }

        return (ViewModelBase)_serviceProvider.GetRequiredService(registration.ViewModelType);
    }
}
