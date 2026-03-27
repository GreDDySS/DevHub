using DevHub.Application.DTOs;
using DevHub.Application.Exceptions;
using DevHub.Domain.Interfaces;

namespace DevHub.Application.UseCases.Projects;

public class UpdateProjectUseCase
{
    private readonly IProjectRepository _repository;

    public UpdateProjectUseCase(IProjectRepository repository)
    {
        _repository = repository;
    }

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

        project.MarkUpdated();
        await _repository.UpdateAsync(project);
    }
}
