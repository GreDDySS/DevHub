using System.Diagnostics;
using System.Reflection;
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
    private readonly Services.ThemeService _themeService;
    private bool _sidebarExpanded = true;
    private bool _hasNewVersion;

    public MainViewModel(IWindowService windowService, Services.ThemeService themeService)
    {
        _windowService = windowService;
        _themeService = themeService;
        Title = "DevHub";
        Width = 1200;
        Height = 800;
        SidebarWidth = new GridLength(200);
        SidebarTextVisibility = Visibility.Visible;
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    }

    public GridLength SidebarWidth { get; private set; }
    public Visibility SidebarTextVisibility { get; private set; }
    public string SidebarTitle => _sidebarExpanded ? "DevHub" : "D";
    public bool ShowFullLogo => _sidebarExpanded;
    public string Version { get; }
    public bool ShowVersion => _sidebarExpanded;
    public bool HasNewVersion { get => _hasNewVersion; }
    public bool ShowUpdateBadge => _sidebarExpanded && _hasNewVersion;
    public bool IsDarkTheme => _themeService.IsDarkTheme;

    [RelayCommand] private void GoToProjects() => _windowService.NavigateTo("projects");
    [RelayCommand] private void GoToLinks() => _windowService.NavigateTo("links");
    [RelayCommand] private void GoToSettings() => _windowService.NavigateTo("settings");
    [RelayCommand] private void ToggleTheme() => _themeService.ToggleTheme();
    [RelayCommand] private void OpenGitHub() => Process.Start(new ProcessStartInfo { FileName = "https://github.com/GreDDySS/DevHub", UseShellExecute = true });

    public void ToggleSidebar()
    {
        _sidebarExpanded = !_sidebarExpanded;
        SidebarWidth = _sidebarExpanded ? new GridLength(200) : new GridLength(64);
        SidebarTextVisibility = _sidebarExpanded ? Visibility.Visible : Visibility.Collapsed;
        OnPropertyChanged(nameof(SidebarWidth));
        OnPropertyChanged(nameof(SidebarTextVisibility));
        OnPropertyChanged(nameof(SidebarTitle));
        OnPropertyChanged(nameof(ShowFullLogo));
        OnPropertyChanged(nameof(ShowVersion));
        OnPropertyChanged(nameof(ShowUpdateBadge));
    }

    public void SetNewVersionAvailable(bool available)
    {
        _hasNewVersion = available;
        OnPropertyChanged(nameof(HasNewVersion));
        OnPropertyChanged(nameof(ShowUpdateBadge));
    }

    public override void OnWindowLoaded() => _windowService.NavigateTo("projects");
}