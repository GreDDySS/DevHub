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
using DevHub.Presentation.Converters;

namespace DevHub.Presentation.ViewModels;

[SingletonViewModel]
public partial class ProjectListViewModel : BaseUserControlViewModel
{
    private readonly GetAllProjectsUseCase _getAllProjects;
    private readonly UpdateProjectUseCase _updateProject;
    private readonly IWindowService _windowService;
    private readonly IProcessLauncher _processLauncher;
    private readonly IAppSettingsStore _settingsStore;
    private readonly DispatcherTimer _debounceTimer;

    public ProjectListViewModel(
        GetAllProjectsUseCase getAllProjects,
        UpdateProjectUseCase updateProject,
        IWindowService windowService,
        IProcessLauncher processLauncher,
        IAppSettingsStore settingsStore)
    {
        _getAllProjects = getAllProjects;
        _updateProject = updateProject;
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

    public override async Task OnNavigatedToAsync()
    {
        await LoadProjectsAsync();
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
    private StatusFilterItem _statusFilter = Statuses[0];

    public static List<StatusFilterItem> Statuses { get; } =
    [
        new("All", null),
        new("Active", ProjectStatus.Active),
        new("Paused", ProjectStatus.Paused),
        new("Completed", ProjectStatus.Completed),
        new("Archived", ProjectStatus.Archived)
    ];

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _activeCount;

    [ObservableProperty]
    private bool _showHidden;

    [ObservableProperty]
    private ViewMode _viewMode = ViewMode.Tiles;

    [RelayCommand]
    private void ToggleViewMode()
    {
        ViewMode = ViewMode == ViewMode.Tiles ? ViewMode.List : ViewMode.Tiles;
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var filter = new ProjectFilter(SearchQuery, null, ShowHidden: ShowHidden);
            var projects = await _getAllProjects.ExecuteAsync(filter);
            var settings = await _settingsStore.LoadAsync();

            var cards = projects.Select(p =>
            {
                var card = new ProjectCardViewModel(p, _processLauncher, _windowService, settings);
                card.OnEditCompleted += () => _ = SafeLoadProjectsAsync();
                card.OnFavoriteToggled += async (id, isFavorite) =>
                {
                    await _updateProject.ExecuteAsync(id, new UpdateProjectRequest(IsFavorite: isFavorite));
                    await SafeLoadProjectsAsync();
                };
                card.OnHiddenToggled += async (id, isHidden) =>
                {
                    await _updateProject.ExecuteAsync(id, new UpdateProjectRequest(IsHidden: isHidden));
                    await SafeLoadProjectsAsync();
                };
                return card;
            }).ToList();

            await Task.WhenAll(cards.Select(card => card.RefreshLastWriteAsync()));

            var statusValue = StatusFilter.Value;

            Projects.Clear();
            foreach (var card in cards)
            {
                if (statusValue is null || card.EffectiveStatus == statusValue)
                    Projects.Add(card);
            }

            UpdateCounts();
        });
    }

    [RelayCommand]
    private void AddProject()
    {
        _windowService.ShowDialog(typeof(AddProjectViewModel));
        _ = SafeLoadProjectsAsync();
    }

    [RelayCommand]
    private void AutoDetect()
    {
        _windowService.ShowDialog(typeof(AutoDetectProjectsViewModel));
        _ = SafeLoadProjectsAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    partial void OnStatusFilterChanged(StatusFilterItem value)
    {
        _ = SafeLoadProjectsAsync();
    }

    partial void OnShowHiddenChanged(bool value)
    {
        _ = SafeLoadProjectsAsync();
    }

    private void UpdateCounts()
    {
        TotalCount = Projects.Count;
        ActiveCount = Projects.Count(p => p.EffectiveStatus == ProjectStatus.Active);
    }
}
