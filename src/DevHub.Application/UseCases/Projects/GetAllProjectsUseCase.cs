using DevHub.Application.DTOs;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Application.UseCases.Projects;

public class GetAllProjectsUseCase
{
    private readonly IProjectRepository _repository;

    public GetAllProjectsUseCase(IProjectRepository repository)
    {
        _repository = repository;
    }

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

        if (!filter.ShowHidden)
            query = query.Where(p => !p.IsHidden);

        if (filter.Status is not null)
            query = query.Where(p => p.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            query = query.Where(p =>
                p.Name.Contains(filter.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(filter.SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false));

        if (filter.Tags?.Count > 0)
            query = query.Where(p => p.Tags.Any(t => filter.Tags.Contains(t)));

        return query.ToList();
    }

    private static ProjectDto MapToDto(Project p) => new(
        p.Id,
        p.Name,
        p.Path,
        p.Description,
        p.Notes,
        p.Language,
        p.Status,
        p.Tags,
        p.PreferredIde,
        p.IsFavorite,
        p.IsHidden,
        p.UpdatedAt);
}
