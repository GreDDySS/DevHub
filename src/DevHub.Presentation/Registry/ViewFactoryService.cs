using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.Registry;

public interface IViewFactoryService
{
    void RegisterAll(IServiceCollection services, Assembly assembly);
    FrameworkElement GetView<TViewModel>() where TViewModel : ViewModelBase;
    Window GetWindow<TViewModel>() where TViewModel : ViewModelBase;
}

public class ViewFactoryService : IViewFactoryService
{
    private readonly ViewRegistry _registry;
    private IServiceProvider? _serviceProvider;
    private ViewModelResolver? _resolver;

    public ViewFactoryService(ViewRegistry registry)
    {
        _registry = registry;
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _resolver = new ViewModelResolver(serviceProvider);
    }

    public void RegisterAll(IServiceCollection services, Assembly assembly)
    {
        var baseTypesToExclude = new[]
        {
            typeof(ViewModelBase),
            typeof(BaseUserControlViewModel),
            typeof(BaseWindowViewModel)
        };

        var allTypes = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract).ToList();

        var viewModels = allTypes
            .Where(t => typeof(ViewModelBase).IsAssignableFrom(t))
            .Where(t => !baseTypesToExclude.Contains(t))
            .OrderByDescending(t => typeof(BaseWindowViewModel).IsAssignableFrom(t))
            .ToList();

        var views = allTypes
            .Where(t => typeof(Window).IsAssignableFrom(t) || typeof(System.Windows.Controls.UserControl).IsAssignableFrom(t))
            .ToList();

        foreach (var vmType in viewModels)
        {
            var isSingleton = vmType.GetCustomAttribute<SingletonViewModelAttribute>() is not null;

            if (isSingleton)
                services.AddSingleton(vmType);
            else
                services.AddTransient(vmType);

            var viewType = FindViewForViewModel(vmType, views);

            if (viewType is null) continue;

            var navAttr = viewType.GetCustomAttribute<NavigationViewAttribute>();

            _registry.Register(new ViewRegistration
            {
                ViewType = viewType,
                ViewModelType = vmType,
                IsNavigation = navAttr is not null,
                NavigationKey = navAttr?.Key,
                IsSingleton = isSingleton
            });
        }
    }

    private static Type? FindViewForViewModel(Type vmType, List<Type> views)
    {
        var expectedViewName = vmType.Name.Replace("ViewModel", "View");
        var expectedWindowName = vmType.Name.Replace("ViewModel", "Window");
        return views.FirstOrDefault(v => v.Name == expectedViewName || v.Name == expectedWindowName);
    }

    public FrameworkElement GetView<TViewModel>() where TViewModel : ViewModelBase
    {
        var registration = _registry.GetByViewModel<TViewModel>()
            ?? throw new InvalidOperationException($"View for {typeof(TViewModel).Name} not found");

        return ResolveView(registration);
    }

    public Window GetWindow<TViewModel>() where TViewModel : ViewModelBase
    {
        var registration = _registry.GetByViewModel<TViewModel>()
            ?? throw new InvalidOperationException($"View for {typeof(TViewModel).Name} not found");

        var viewModel = Resolve(registration);
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
        var viewModel = Resolve(registration);
        var view = (FrameworkElement)Activator.CreateInstance(registration.ViewType)!;
        view.DataContext = viewModel;
        return view;
    }

    private ViewModelBase Resolve(ViewRegistration registration)
    {
        if (_resolver is null)
            throw new InvalidOperationException("ServiceProvider not set. Call SetServiceProvider first.");

        return _resolver.Resolve(registration);
    }
}
