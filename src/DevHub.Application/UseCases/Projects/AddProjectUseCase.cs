using DevHub.Application.DTOs;
using DevHub.Application.Exceptions;
using DevHub.Application.Interfaces;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Application.UseCases.Projects;

public class AddProjectUseCase(IProjectRepository repository) : IAddProjectUseCase
{
    public async Task<Project> ExecuteAsync(CreateProjectRequest request, CancellationToken ct = default)
    {
        Validate(request);

        var project = Project.Create(request.Name, request.Path, request.Language);

        await repository.AddAsync(project, ct);
        return project;
    }

    private static void Validate(CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Project name is required.");

        if (string.IsNullOrWhiteSpace(request.Path))
            throw new ValidationException("Project path is required.");
    }
}
