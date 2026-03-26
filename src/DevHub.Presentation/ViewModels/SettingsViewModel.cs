using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.Interfaces;
using DevHub.Infrastructure.Storage;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;
using DevHub.Presentation.Services;

namespace DevHub.Presentation.ViewModels;

[SingletonViewModel]
public partial class SettingsViewModel : BaseUserControlViewModel
{
    private readonly JsonSettingsStore _settingsStore;
    private readonly AutostartService _autostartService;
    private readonly IWindowService _windowService;

    public SettingsViewModel(
        JsonSettingsStore settingsStore,
        AutostartService autostartService,
        IWindowService windowService)
    {
        _settingsStore = settingsStore;
        _autostartService = autostartService;
        _windowService = windowService;
        Settings = _settingsStore.Load();
    }

    [ObservableProperty]
    private AppSettings _settings;

    [RelayCommand]
    private void Save()
    {
        try
        {
            _autostartService.SetEnabled(Settings.AutostartEnabled);
            _settingsStore.Save(Settings);
            _windowService.ShowNotification("Settings", "Settings saved successfully");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private void BrowseVsCode()
    {
        var path = _windowService.OpenFileDialog("Executables (*.exe)|*.exe");
        if (path is not null) Settings.VsCodePath = path;
    }

    [RelayCommand]
    private void BrowseVisualStudio()
    {
        var path = _windowService.OpenFileDialog("Executables (*.exe)|*.exe");
        if (path is not null) Settings.VisualStudioPath = path;
    }

    [RelayCommand]
    private void BrowseRider()
    {
        var path = _windowService.OpenFileDialog("Executables (*.exe)|*.exe");
        if (path is not null) Settings.RiderPath = path;
    }

    [RelayCommand]
    private void AutoDetect()
    {
        var defaults = AppSettings.DetectDefaults();
        if (!string.IsNullOrEmpty(defaults.VsCodePath))
            Settings.VsCodePath = defaults.VsCodePath;
        if (!string.IsNullOrEmpty(defaults.RiderPath))
            Settings.RiderPath = defaults.RiderPath;
    }
}
