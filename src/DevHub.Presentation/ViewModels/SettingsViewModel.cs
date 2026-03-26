using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.Interfaces;
using DevHub.Domain.Enums;
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
        Ides = new ObservableCollection<IdeEntry>(Settings.Ides);
    }

    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private ObservableCollection<IdeEntry> _ides;

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
    private void Save()
    {
        try
        {
            Settings.Ides = Ides.ToList();
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
    private void AddIde()
    {
        var path = _windowService.OpenFileDialog("Executables (*.exe)|*.exe");
        if (path is null) return;

        var name = Path.GetFileNameWithoutExtension(path);
        Ides.Add(new IdeEntry(name, path));
    }

    [RelayCommand]
    private void RemoveIde(IdeEntry entry)
    {
        Ides.Remove(entry);

        if (Settings.DefaultIdeIndex >= Ides.Count)
            Settings.DefaultIdeIndex = Math.Max(0, Ides.Count - 1);
    }

    [RelayCommand]
    private void AutoDetect()
    {
        var scanner = new IdeScanner();
        var detected = scanner.Scan();

        var existing = Ides.Select(i => i.Path).ToHashSet();
        foreach (var ide in detected)
        {
            if (!existing.Contains(ide.Path))
                Ides.Add(ide);
        }

        if (Ides.Count > 0 && Settings.DefaultIdeIndex >= Ides.Count)
            Settings.DefaultIdeIndex = 0;
    }
}
