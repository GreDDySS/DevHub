using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.DTOs;
using DevHub.Application.Interfaces;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Enums;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

public partial class AddProjectViewModel : BaseWindowViewModel
{
    private readonly AddProjectUseCase _addProjectUseCase;
    private readonly UpdateProjectUseCase _updateProjectUseCase;
    private readonly IWindowService _windowService;

    public AddProjectViewModel(
        AddProjectUseCase addProjectUseCase,
        UpdateProjectUseCase updateProjectUseCase,
        IWindowService windowService)
    {
        _addProjectUseCase = addProjectUseCase;
        _updateProjectUseCase = updateProjectUseCase;
        _windowService = windowService;
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

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private ProgrammingLanguage _language = ProgrammingLanguage.CSharp;

    public bool IsEditing => _editingProjectId.HasValue;

    public Array Languages => Enum.GetValues<ProgrammingLanguage>();

    public void SetEditMode(Guid projectId, string name, string path, string? description, ProgrammingLanguage language)
    {
        _editingProjectId = projectId;
        Name = name;
        Path = path;
        Description = description;
        Language = language;
        Title = "Edit Project";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            if (IsEditing)
            {
                await _updateProjectUseCase.ExecuteAsync(_editingProjectId!.Value,
                    new UpdateProjectRequest(Name, Description, Language: Language));
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
    private void Cancel()
    {
        _windowService.CloseWindow(this);
    }

    [ObservableProperty]
    private bool _saveCompleted;
}
