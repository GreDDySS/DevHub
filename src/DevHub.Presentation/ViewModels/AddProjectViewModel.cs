using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.DTOs;
using DevHub.Application.Interfaces;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

public partial class AddProjectViewModel : BaseWindowViewModel
{
    private readonly IAddProjectUseCase _addProjectUseCase;
    private readonly IUpdateProjectUseCase _updateProjectUseCase;
    private readonly IWindowService _windowService;
    private readonly IAppSettingsStore _settingsStore;
    private readonly List<IdeEntry> _cachedIdes = [];

    public AddProjectViewModel(
        IAddProjectUseCase addProjectUseCase,
        IUpdateProjectUseCase updateProjectUseCase,
        IWindowService windowService,
        IAppSettingsStore settingsStore)
    {
        _addProjectUseCase = addProjectUseCase;
        _updateProjectUseCase = updateProjectUseCase;
        _windowService = windowService;
        _settingsStore = settingsStore;
        Title = "Add Project";
        Width = 500;
        Height = 450;
    }

    private Guid? _editingProjectId;

    [ObservableProperty]
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    private string _name = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Path is required")]
    private string _path = string.Empty;

    [ObservableProperty] private string? _description;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private ProgrammingLanguage _language = ProgrammingLanguage.CSharp;
    [ObservableProperty] private ObservableCollection<IdeEntry> _availableIdes = [];
    [ObservableProperty] private IdeEntry? _selectedIde;
    [ObservableProperty] private ObservableCollection<string> _tags = [];
    [ObservableProperty] private string _newTag = string.Empty;
    [ObservableProperty] private bool _saveCompleted;

    public bool IsEditing => _editingProjectId.HasValue;
    public Array Languages => Enum.GetValues<ProgrammingLanguage>();

    private async Task LoadIdesAsync()
    {
        try
        {
            if (_cachedIdes.Count == 0)
            {
                var settings = await _settingsStore.LoadAsync();
                _cachedIdes.AddRange(settings.Ides);
            }
            AvailableIdes = new ObservableCollection<IdeEntry>(_cachedIdes);
        }
        catch { /* IDE loading is non-critical */ }
    }

    public void SetEditMode(Guid projectId, string name, string path, string? description,
        ProgrammingLanguage language, string? notes, string? preferredIde, List<string> tags)
    {
        _editingProjectId = projectId;
        Name = name;
        Path = path;
        Description = description;
        Language = language;
        Notes = notes;

        LoadIdesSync();
        SelectedIde = AvailableIdes.FirstOrDefault(i => i.Path == preferredIde);
        Tags = new ObservableCollection<string>(tags);
        Title = "Edit Project";
        Height = 580;
    }

    private void LoadIdesSync()
    {
        try
        {
            if (_cachedIdes.Count == 0)
            {
                var settings = Task.Run(() => _settingsStore.LoadAsync()).GetAwaiter().GetResult();
                _cachedIdes.AddRange(settings.Ides);
            }
            AvailableIdes = new ObservableCollection<IdeEntry>(_cachedIdes);
        }
        catch { /* IDE loading is non-critical */ }
    }

    [RelayCommand]
    private void AddTag()
    {
        var tag = NewTag.Trim();
        if (!string.IsNullOrEmpty(tag) && !Tags.Contains(tag))
        {
            Tags.Add(tag);
            NewTag = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveTag(string tag) => Tags.Remove(tag);

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var preferredIde = SelectedIde?.Path;

            if (IsEditing)
            {
                await _updateProjectUseCase.ExecuteAsync(_editingProjectId!.Value,
                    new UpdateProjectRequest(Name, Description, Notes, Language: Language,
                        Tags: Tags.ToList(), PreferredIde: preferredIde));
            }
            else
            {
                await _addProjectUseCase.ExecuteAsync(
                    new CreateProjectRequest(Name, Path, Description, Language));
            }

            SaveCompleted = true;
            _windowService.CloseWindow(this);
        });
    }

    [RelayCommand]
    private void BrowsePath()
    {
        var path = _windowService.OpenFolderDialog();
        if (path is not null)
            Path = path;
    }

    [RelayCommand]
    private void Cancel() => _windowService.CloseWindow(this);
}
