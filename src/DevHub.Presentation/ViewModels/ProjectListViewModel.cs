using System.Collections.ObjectModel;
using System.Windows.Threading;
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
    private readonly IAppSettingsStore _settingsStore;
    private readonly DispatcherTimer _debounceTimer;

    public ProjectListViewModel(
        GetAllProjectsUseCase getAllProjects,
        IWindowService windowService,
        IProcessLauncher processLauncher,
        IAppSettingsStore settingsStore)
    {
        _getAllProjects = getAllProjects;
        _windowService = windowService;
        _processLauncher = processLauncher;
        _settingsStore = settingsStore;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await SafeLoadProjectsAsync();
        };
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await SafeLoadProjectsAsync();
    }

    private async Task SafeLoadProjectsAsync()
    {
        try
        {
            await LoadProjectsAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load projects: {ex.Message}";
        }
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
            var settings = await _settingsStore.LoadAsync();

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
        _ = SafeLoadProjectsAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    partial void OnStatusFilterChanged(ProjectStatus? value)
    {
        _ = SafeLoadProjectsAsync();
    }

    private void UpdateCounts()
    {
        TotalCount = Projects.Count;
        ActiveCount = Projects.Count(p => p.Status == ProjectStatus.Active);
    }
}
