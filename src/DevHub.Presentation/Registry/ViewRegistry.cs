using DevHub.Presentation.Base;

namespace DevHub.Presentation.Registry;

public class ViewRegistry
{
    private readonly Dictionary<Type, ViewRegistration> _viewModelToView = new();
    private readonly Dictionary<string, ViewRegistration> _navigationViews = new();

    public void Register(ViewRegistration registration)
    {
        _viewModelToView[registration.ViewModelType] = registration;

        if (registration.IsNavigation && registration.NavigationKey is not null)
        {
            _navigationViews[registration.NavigationKey] = registration;
        }
    }

    public ViewRegistration? GetByViewModel<TViewModel>() where TViewModel : ViewModelBase
        => GetByViewModel(typeof(TViewModel));

    public ViewRegistration? GetByViewModel(Type viewModelType)
        => _viewModelToView.GetValueOrDefault(viewModelType);

    public ViewRegistration? GetByKey(string key)
        => _navigationViews.GetValueOrDefault(key);

    public IReadOnlyDictionary<string, ViewRegistration> GetAllNavigationViews()
        => _navigationViews;
}
