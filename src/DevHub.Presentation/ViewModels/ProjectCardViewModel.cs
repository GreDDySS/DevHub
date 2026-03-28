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
    private readonly AppSettings _settings;

    public ProjectDto Dto { get; }

    public ProjectCardViewModel(ProjectDto dto, IProcessLauncher processLauncher, IWindowService windowService, AppSettings settings)
    {
        Dto = dto;
        _processLauncher = processLauncher;
        _windowService = windowService;
        _settings = settings;
    }

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
    public DateTime UpdatedAt => Dto.UpdatedAt;

    public string StatusColor => Status switch
    {
        ProjectStatus.Active => "#4CAF50",
        ProjectStatus.Completed => "#2196F3",
        ProjectStatus.Paused => "#FF9800",
        ProjectStatus.Archived => "#9E9E9E",
        _ => "#000000"
    };

    public string LanguageIcon => Language switch
    {
        ProgrammingLanguage.CSharp => "🔷",
        ProgrammingLanguage.Python => "🐍",
        ProgrammingLanguage.Rust => "🦀",
        ProgrammingLanguage.JavaScript => "🟨",
        ProgrammingLanguage.TypeScript => "🔵",
        ProgrammingLanguage.Go => "🐹",
        ProgrammingLanguage.Java => "☕",
        ProgrammingLanguage.Cpp => "⚙️",
        _ => "📄"
    };

    public string FormattedUpdatedAt => UpdatedAt.ToString("dd.MM.yyyy HH:mm");

    [RelayCommand]
    private void OpenInExplorer()
    {
        try
        {
            _processLauncher.OpenInExplorer(Path);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private void OpenInIde()
    {
        try
        {
            string? idePath = null;

            if (!string.IsNullOrEmpty(PreferredIde))
                idePath = PreferredIde;
            else if (_settings.Ides.Count > 0 && _settings.DefaultIdeIndex < _settings.Ides.Count)
                idePath = _settings.Ides[_settings.DefaultIdeIndex].Path;

            if (string.IsNullOrEmpty(idePath))
            {
                ErrorMessage = "No IDE configured. Add one in Settings.";
                HasError = true;
                return;
            }

            _processLauncher.OpenInIde(idePath, Path);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private void OpenConsole()
    {
        try
        {
            _processLauncher.OpenConsole(Path);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    public event Action? OnEditCompleted;

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
