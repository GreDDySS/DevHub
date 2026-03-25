using DevHub.Presentation.Base;

namespace DevHub.Presentation.Registry;

public class ViewRegistration
{
    public required Type ViewType { get; init; }
    public required Type ViewModelType { get; init; }
    public bool IsNavigation { get; init; }
    public string? NavigationKey { get; init; }
    public bool IsSingleton { get; init; }
}
