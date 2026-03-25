# Project Management Module

## Обзор

Модуль управления проектами — ядро MVP. Отвечает за CRUD-операции над проектами и отображение списка.

## Use Cases

### GetAllProjectsUseCase

```csharp
public class GetAllProjectsUseCase
{
    private readonly IProjectRepository _repository;

    public GetAllProjectsUseCase(IProjectRepository repository)
        => _repository = repository;

    public async Task<List<ProjectDto>> ExecuteAsync(ProjectFilter? filter = null)
    {
        var projects = await _repository.GetAllAsync();

        if (filter is not null)
            projects = ApplyFilter(projects, filter);

        return projects
            .OrderByDescending(p => p.IsFavorite)
            .ThenByDescending(p => p.UpdatedAt)
            .Select(MapToDto)
            .ToList();
    }

    private static List<Project> ApplyFilter(List<Project> projects, ProjectFilter filter)
    {
        var query = projects.AsEnumerable();

        if (filter.Status is not null)
            query = query.Where(p => p.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            query = query.Where(p =>
                p.Name.Contains(filter.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Description?.Contains(filter.SearchQuery, StringComparison.OrdinalIgnoreCase) == true);

        if (filter.Tags?.Count > 0)
            query = query.Where(p => p.Tags.Any(t => filter.Tags.Contains(t)));

        return query.ToList();
    }
}
```

### AddProjectUseCase

```csharp
public class AddProjectUseCase
{
    private readonly IProjectRepository _repository;

    public async Task<Project> ExecuteAsync(CreateProjectRequest request)
    {
        Validate(request);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Path = request.Path.Trim(),
            Description = request.Description?.Trim(),
            Language = request.Language,
            Status = ProjectStatus.Active,
            Tags = new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(project);
        return project;
    }

    private static void Validate(CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Project name is required");

        if (string.IsNullOrWhiteSpace(request.Path))
            throw new ValidationException("Project path is required");

        if (!Directory.Exists(request.Path))
            throw new ValidationException($"Directory does not exist: {request.Path}");
    }
}
```

### UpdateProjectUseCase

```csharp
public class UpdateProjectUseCase
{
    private readonly IProjectRepository _repository;

    public async Task ExecuteAsync(Guid id, UpdateProjectRequest request)
    {
        var project = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Project {id} not found");

        if (request.Name is not null) project.Name = request.Name.Trim();
        if (request.Description is not null) project.Description = request.Description.Trim();
        if (request.Notes is not null) project.Notes = request.Notes.Trim();
        if (request.Status is not null) project.Status = request.Status.Value;
        if (request.Language is not null) project.Language = request.Language.Value;
        if (request.Tags is not null) project.Tags = request.Tags;
        if (request.PreferredIde is not null) project.PreferredIde = request.PreferredIde;
        if (request.IsFavorite is not null) project.IsFavorite = request.IsFavorite.Value;

        project.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(project);
    }
}
```

### DeleteProjectUseCase

```csharp
public class DeleteProjectUseCase
{
    private readonly IProjectRepository _repository;

    public async Task ExecuteAsync(Guid id)
    {
        var project = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Project {id} not found");

        await _repository.DeleteAsync(id);
    }
}
```

## ViewModel — ProjectListViewModel

```csharp
public partial class ProjectListViewModel : ViewModelBase
{
    private readonly GetAllProjectsUseCase _getAllProjects;
    private readonly DeleteProjectUseCase _deleteProject;

    public ObservableCollection<ProjectCardViewModel> Projects { get; } = new();

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
        var filter = new ProjectFilter
        {
            SearchQuery = SearchQuery,
            Status = StatusFilter
        };

        var projects = await _getAllProjects.ExecuteAsync(filter);
        
        Projects.Clear();
        foreach (var p in projects)
            Projects.Add(new ProjectCardViewModel(p));

        UpdateCounts();
    }

    [RelayCommand]
    private async Task DeleteProjectAsync(Guid id)
    {
        await _deleteProject.ExecuteAsync(id);
        await LoadProjectsAsync();
    }

    partial void OnSearchQueryChanged(string value)
        => LoadProjectsAsyncCommand.ExecuteAsync(null);

    partial void OnStatusFilterChanged(ProjectStatus? value)
        => LoadProjectsAsyncCommand.ExecuteAsync(null);

    private void UpdateCounts()
    {
        TotalCount = Projects.Count;
        ActiveCount = Projects.Count(p => p.Status == ProjectStatus.Active);
    }
}
```

## DTOs

```csharp
public record ProjectDto(
    Guid Id,
    string Name,
    string Path,
    string? Description,
    string? Notes,
    ProgrammingLanguage Language,
    ProjectStatus Status,
    List<string> Tags,
    string? PreferredIde,
    bool IsFavorite,
    DateTime UpdatedAt);

public record CreateProjectRequest(
    string Name,
    string Path,
    string? Description,
    ProgrammingLanguage Language);

public record UpdateProjectRequest(
    string? Name = null,
    string? Description = null,
    string? Notes = null,
    ProjectStatus? Status = null,
    ProgrammingLanguage? Language = null,
    List<string>? Tags = null,
    string? PreferredIde = null,
    bool? IsFavorite = null);

public record ProjectFilter(
    string? SearchQuery = null,
    ProjectStatus? Status = null,
    List<string>? Tags = null);
```

## Process Launcher

```csharp
public class ProcessLauncher : IProcessLauncher
{
    public void OpenInExplorer(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = path,
            UseShellExecute = true
        });
    }

    public void OpenInIde(string idePath, string projectPath)
    {
        if (!File.Exists(idePath))
            throw new FileNotFoundException("IDE not found", idePath);

        Process.Start(new ProcessStartInfo
        {
            FileName = idePath,
            Arguments = $"\"{projectPath}\"",
            UseShellExecute = true
        });
    }
}
```

## Связанные документы

- [Architecture Overview](../03-Architecture/architecture-overview.md)
- [Data Model](../03-Architecture/data-model.md)
- [Development Roadmap](../06-Roadmap/development-roadmap.md)
- [Global Hotkey Implementation](global-hotkey-implementation.md)
