using DevHub.Application.DTOs;
using DevHub.Application.Exceptions;
using DevHub.Application.Interfaces;
using DevHub.Domain.Interfaces;

namespace DevHub.Application.UseCases.Projects;

public class UpdateProjectUseCase(IProjectRepository repository) : IUpdateProjectUseCase
{
    public async Task ExecuteAsync(Guid id, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var project = await repository.GetByIdAsync(ct, id)
            ?? throw new NotFoundException($"Project {id} not found.");

        if (request.Name is not null) project.Rename(request.Name);
        if (request.Description is not null) project.UpdateDescription(request.Description);
        if (request.Notes is not null) project.UpdateNotes(request.Notes);
        if (request.Status is not null) project.ChangeStatus(request.Status.Value);
        if (request.Language is not null) project.ChangeLanguage(request.Language.Value);
        if (request.Tags is not null) project.SetTags(request.Tags);
        if (request.PreferredIde is not null) project.SetPreferredIde(request.PreferredIde);
        if (request.IsFavorite is not null && request.IsFavorite.Value != project.IsFavorite) project.ToggleFavorite();
        if (request.IsHidden is not null && request.IsHidden.Value != project.IsHidden) project.ToggleHidden();

        await repository.UpdateAsync(project, ct);
    }
}
