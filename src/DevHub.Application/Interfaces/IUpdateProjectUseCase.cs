using DevHub.Application.DTOs;

namespace DevHub.Application.Interfaces;

public interface IUpdateProjectUseCase
{
    Task ExecuteAsync(Guid id, UpdateProjectRequest request, CancellationToken ct = default);
}
