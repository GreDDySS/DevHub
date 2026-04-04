using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.Interfaces;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;
using DevHub.Presentation.Services;

namespace DevHub.Presentation.ViewModels;

[SingletonViewModel]
[NavigationView("settings")]
public partial class SettingsViewModel : BaseUserControlViewModel
{
    private readonly IAppSettingsStore _settingsStore;
    private readonly IAutostartService _autostartService;
    private readonly IWindowService _windowService;
    private readonly IIdeScanner _ideScanner;

    public SettingsViewModel(
        IAppSettingsStore settingsStore,
        IAutostartService autostartService,
        IWindowService windowService,
        IIdeScanner ideScanner)
    {
        _settingsStore = settingsStore;
        _autostartService = autostartService;
        _windowService = windowService;
        _ideScanner = ideScanner;
    }

    public override async Task OnNavigatedToAsync() => await InitializeAsync();

    private async Task InitializeAsync()
    {
        try
        {
            Settings = await _settingsStore.LoadAsync();
            Ides = new ObservableCollection<IdeEntry>(Settings.Ides);
            SelectedIdeIndex = Settings.DefaultIdeIndex;
        }
        catch (Exception ex)
        {
            Settings = new AppSettings();
            Ides = new ObservableCollection<IdeEntry>();
            HasError = true;
            ErrorMessage = $"Failed to load settings: {ex.Message}";
        }
    }

    [ObservableProperty] private AppSettings _settings = new();
    [ObservableProperty] private ObservableCollection<IdeEntry> _ides = [];
    [ObservableProperty] private int _selectedIdeIndex;

    partial void OnSelectedIdeIndexChanged(int value)
    {
        Settings.DefaultIdeIndex = value;
    }

    public bool IsCloseExit
    {
        get => Settings.CloseAction == CloseAction.Exit;
        set { if (value) Settings.CloseAction = CloseAction.Exit; OnPropertyChanged(); }
    }

    public bool IsCloseTray
    {
        get => Settings.CloseAction == CloseAction.MinimizeToTray;
        set { if (value) Settings.CloseAction = CloseAction.MinimizeToTray; OnPropertyChanged(); }
    }

    public bool IsCloseAsk
    {
        get => Settings.CloseAction == CloseAction.Ask;
        set { if (value) Settings.CloseAction = CloseAction.Ask; OnPropertyChanged(); }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            Settings.Ides = Ides.ToList();
            Settings.DefaultIdeIndex = SelectedIdeIndex;
            _autostartService.SetEnabled(Settings.AutostartEnabled);
            await _settingsStore.SaveAsync(Settings);
            _windowService.ShowNotification("Settings", "Settings saved successfully");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private void AddIde()
    {
        var path = _windowService.OpenFileDialog("Executables (*.exe)|*.exe");
        if (path is null) return;

        var name = Path.GetFileNameWithoutExtension(path);
        Ides.Add(new IdeEntry(name, path));

        if (Ides.Count == 1)
            SelectedIdeIndex = 0;
    }

    [RelayCommand]
    private void RemoveIde(IdeEntry entry)
    {
        var index = Ides.IndexOf(entry);
        Ides.Remove(entry);

        if (Ides.Count == 0)
            SelectedIdeIndex = 0;
        else if (index >= Ides.Count)
            SelectedIdeIndex = Ides.Count - 1;
        else if (index == SelectedIdeIndex)
            SelectedIdeIndex = Math.Max(0, index);
    }

    [RelayCommand]
    private void AutoDetect()
    {
        var detected = _ideScanner.Scan();
        var existing = Ides.Select(i => i.Path).ToHashSet();
        foreach (var ide in detected)
        {
            if (!existing.Contains(ide.Path))
                Ides.Add(ide);
        }

        if (Ides.Count > 0 && SelectedIdeIndex >= Ides.Count)
            SelectedIdeIndex = 0;
    }
}
