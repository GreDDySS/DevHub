using DevHub.Application.DTOs;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Application.Interfaces;

public interface IGetAllProjectsUseCase
{
    Task<List<ProjectDto>> ExecuteAsync(ProjectFilter? filter, CancellationToken ct = default);
}
