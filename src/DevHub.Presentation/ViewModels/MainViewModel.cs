using CommunityToolkit.Mvvm.Input;
using DevHub.Application.Interfaces;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

[SingletonViewModel]
public partial class MainViewModel : BaseWindowViewModel
{
    private readonly IWindowService _windowService;

    public MainViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        Title = "DevHub";
        Width = 1200;
        Height = 800;
    }

    [RelayCommand] private void GoToProjects() => _windowService.NavigateTo("projects");
    [RelayCommand] private void GoToLinks() => _windowService.NavigateTo("links");
    [RelayCommand] private void GoToSettings() => _windowService.NavigateTo("settings");

    public override void OnWindowLoaded() => _windowService.NavigateTo("projects");
}
