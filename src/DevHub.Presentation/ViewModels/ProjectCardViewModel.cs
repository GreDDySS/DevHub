using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.DTOs;
using DevHub.Application.Interfaces;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

public partial class ProjectCardViewModel : BaseUserControlViewModel
{
    private readonly IProcessLauncher _processLauncher;
    private readonly IWindowService _windowService;
    private readonly List<IdeEntry> _ides;
    private readonly int _defaultIdeIndex;

    public ProjectCardViewModel(
        ProjectDto dto,
        IProcessLauncher processLauncher,
        IWindowService windowService,
        List<IdeEntry> ides,
        int defaultIdeIndex)
    {
        Dto = dto;
        _processLauncher = processLauncher;
        _windowService = windowService;
        _ides = ides;
        _defaultIdeIndex = defaultIdeIndex;
    }

    public ProjectDto Dto { get; }

    public Guid Id => Dto.Id;
    public string Name => Dto.Name;
    public string Path => Dto.Path;
    public string? Description => Dto.Description;
    public string? Notes => Dto.Notes;
    public ProgrammingLanguage Language => Dto.Language;
    public ProjectStatus Status => Dto.Status;
    public List<string> Tags => Dto.Tags;
    public string? PreferredIde => Dto.PreferredIde;
    public bool IsFavorite => Dto.IsFavorite;
    public bool IsHidden => Dto.IsHidden;
    public DateTime UpdatedAt => Dto.UpdatedAt;

    private DateTime? _lastFileWrite;

    public DateTime? LastFileWrite
    {
        get => _lastFileWrite;
        private set
        {
            _lastFileWrite = value;
            OnPropertyChanged(nameof(EffectiveStatus));
        }
    }

    public async Task RefreshLastWriteAsync(CancellationToken ct = default)
    {
        try
        {
            LastFileWrite = await Task.Run(() => _processLauncher.GetLastWriteTime(Path), ct);
        }
        catch { /* Ignore file system errors */ }
    }

    public ProjectStatus EffectiveStatus
    {
        get
        {
            if (Status == ProjectStatus.Active)
            {
                var checkDate = LastFileWrite ?? UpdatedAt;
                if (DateTime.UtcNow - checkDate > TimeSpan.FromDays(14))
                    return ProjectStatus.Paused;
            }
            return Status;
        }
    }

    public string StatusText => EffectiveStatus.ToString();

    public string FormattedUpdatedAt => UpdatedAt.ToString("dd.MM.yyyy HH:mm");

    public Action? OnEditCompleted { get; set; }
    public Action<Guid, bool>? OnFavoriteToggled { get; set; }
    public Action<Guid, bool>? OnHiddenToggled { get; set; }

    [RelayCommand]
    private void OpenInExplorer()
    {
        try { _processLauncher.OpenInExplorer(Path); }
        catch (Exception ex) { ErrorMessage = ex.Message; HasError = true; }
    }

    [RelayCommand]
    private void OpenInIde()
    {
        try
        {
            string? idePath = null;

            if (!string.IsNullOrEmpty(PreferredIde))
                idePath = PreferredIde;
            else if (_ides.Count > 0 && _defaultIdeIndex < _ides.Count)
                idePath = _ides[_defaultIdeIndex].Path;

            if (string.IsNullOrEmpty(idePath))
            {
                ErrorMessage = "No IDE configured. Add one in Settings.";
                HasError = true;
                return;
            }

            _processLauncher.OpenInIde(idePath, Path);
        }
        catch (Exception ex) { ErrorMessage = ex.Message; HasError = true; }
    }

    [RelayCommand]
    private void OpenConsole()
    {
        try { _processLauncher.OpenConsole(Path); }
        catch (Exception ex) { ErrorMessage = ex.Message; HasError = true; }
    }

    [RelayCommand]
    private void ToggleFavorite() => OnFavoriteToggled?.Invoke(Id, !IsFavorite);

    [RelayCommand]
    private void ToggleHidden() => OnHiddenToggled?.Invoke(Id, !IsHidden);

    [RelayCommand]
    private void Edit()
    {
        _windowService.ShowDialog(typeof(AddProjectViewModel), vm =>
        {
            if (vm is AddProjectViewModel editVm)
                editVm.SetEditMode(Id, Name, Path, Description, Language, Notes, PreferredIde, Tags);
        });
        OnEditCompleted?.Invoke();
    }
}
