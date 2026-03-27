using DevHub.Application.DTOs;
using DevHub.Application.Exceptions;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Application.UseCases.Projects;

public class AddProjectUseCase
{
    private readonly IProjectRepository _repository;

    public AddProjectUseCase(IProjectRepository repository)
    {
        _repository = repository;
    }

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
    }
}
