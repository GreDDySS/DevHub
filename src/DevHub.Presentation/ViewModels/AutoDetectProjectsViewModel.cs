using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.Interfaces;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Models;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

public partial class AutoDetectProjectsViewModel : BaseWindowViewModel
{
    private readonly DetectProjectsUseCase _detectUseCase;
    private readonly AddProjectUseCase _addProjectUseCase;
    private readonly IWindowService _windowService;

    public AutoDetectProjectsViewModel(
        DetectProjectsUseCase detectUseCase,
        AddProjectUseCase addProjectUseCase,
        IWindowService windowService)
    {
        _detectUseCase = detectUseCase;
        _addProjectUseCase = addProjectUseCase;
        _windowService = windowService;
        Title = "Auto-detect Projects";
        Width = 600;
        Height = 500;
    }

    [ObservableProperty]
    private string? _rootPath;

    [ObservableProperty]
    private ObservableCollection<Project> _detectedProjects = [];

    [ObservableProperty]
    private bool _hasDetected;

    [ObservableProperty]
    private int _addedCount;

    [ObservableProperty]
    private bool _isDone;

    [RelayCommand]
    private void BrowseFolder()
    {
        var path = _windowService.OpenFolderDialog();
        if (path is not null)
        {
            RootPath = path;
            Scan();
        }
    }

    [RelayCommand]
    private void Scan()
    {
        if (string.IsNullOrWhiteSpace(RootPath))
            return;

        var projects = _detectUseCase.Execute(RootPath);
        DetectedProjects = new ObservableCollection<Project>(projects);
        HasDetected = projects.Count > 0;
    }

    [RelayCommand]
    private async Task AddAllAsync()
    {
        AddedCount = 0;
        foreach (var project in DetectedProjects.Where(p => !p.IsHidden))
        {
            await _addProjectUseCase.ExecuteAsync(new Application.DTOs.CreateProjectRequest(
                project.Name, project.Path, project.Description, project.Language));
            AddedCount++;
        }
        IsDone = true;
    }

    [RelayCommand]
    private void RemoveProject(Project project)
    {
        DetectedProjects.Remove(project);
    }

    [RelayCommand]
    private void Close()
    {
        _windowService.CloseWindow(this);
    }
}
