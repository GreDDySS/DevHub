using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHub.Application.DTOs;
using DevHub.Application.Interfaces;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Infrastructure.Storage;
using DevHub.Presentation.Attributes;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

[SingletonViewModel]
public partial class ProjectListViewModel : BaseUserControlViewModel
{
    private readonly GetAllProjectsUseCase _getAllProjects;
    private readonly IWindowService _windowService;
    private readonly IProcessLauncher _processLauncher;
    private readonly JsonSettingsStore _settingsStore;
    private readonly System.Threading.Timer _debounceTimer;

    public ProjectListViewModel(
        GetAllProjectsUseCase getAllProjects,
        IWindowService windowService,
        IProcessLauncher processLauncher,
        JsonSettingsStore settingsStore)
    {
        _getAllProjects = getAllProjects;
        _windowService = windowService;
        _processLauncher = processLauncher;
        _settingsStore = settingsStore;
        _debounceTimer = new System.Threading.Timer(_ => _ = LoadProjectsAsync(), null, Timeout.Infinite, Timeout.Infinite);
        _ = LoadProjectsAsync();
    }

    [ObservableProperty]
    private ObservableCollection<ProjectCardViewModel> _projects = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ProjectStatus? _statusFilter;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _activeCount;

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var filter = new ProjectFilter(SearchQuery, StatusFilter);
            var projects = await _getAllProjects.ExecuteAsync(filter);
            var settings = _settingsStore.Load();

            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(new ProjectCardViewModel(p, _processLauncher, settings));

            UpdateCounts();
        });
    }

    [RelayCommand]
    private void AddProject()
    {
        _windowService.ShowDialog(typeof(AddProjectViewModel));
        _ = LoadProjectsAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        _debounceTimer.Change(300, Timeout.Infinite);
    }

    partial void OnStatusFilterChanged(ProjectStatus? value)
    {
        _ = LoadProjectsAsync();
    }

    private void UpdateCounts()
    {
        TotalCount = Projects.Count;
        ActiveCount = Projects.Count(p => p.Status == ProjectStatus.Active);
    }
}
