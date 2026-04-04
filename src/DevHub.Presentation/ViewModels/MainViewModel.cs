using System.Windows;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.Interfaces;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

[SingletonViewModel]
public partial class MainViewModel : BaseWindowViewModel
{
    private readonly IWindowService _windowService;
    private bool _sidebarExpanded = true;

    public MainViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        Title = "DevHub";
        Width = 1200;
        Height = 800;
        SidebarWidth = new GridLength(200);
        SidebarTextVisibility = Visibility.Visible;
    }

    public GridLength SidebarWidth { get; private set; }
    public Visibility SidebarTextVisibility { get; private set; }

    [RelayCommand] private void GoToProjects() => _windowService.NavigateTo("projects");
    [RelayCommand] private void GoToLinks() => _windowService.NavigateTo("links");
    [RelayCommand] private void GoToSettings() => _windowService.NavigateTo("settings");

    public void ToggleSidebar()
    {
        _sidebarExpanded = !_sidebarExpanded;
        SidebarWidth = _sidebarExpanded ? new GridLength(200) : new GridLength(72);
        SidebarTextVisibility = _sidebarExpanded ? Visibility.Visible : Visibility.Collapsed;
        OnPropertyChanged(nameof(SidebarWidth));
        OnPropertyChanged(nameof(SidebarTextVisibility));
    }

    public override void OnWindowLoaded() => _windowService.NavigateTo("projects");
}
