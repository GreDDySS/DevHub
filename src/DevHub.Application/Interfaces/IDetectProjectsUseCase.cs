using DevHub.Domain.Models;

namespace DevHub.Application.Interfaces;

public interface IDetectProjectsUseCase
{
    Task<List<Project>> ExecuteAsync(string rootPath, CancellationToken ct = default);
}
