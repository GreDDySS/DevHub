using Microsoft.Extensions.DependencyInjection;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.Registry;

public class ViewModelResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, object> _singletonCache = new();

    public ViewModelResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ViewModelBase Resolve(ViewRegistration registration)
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
