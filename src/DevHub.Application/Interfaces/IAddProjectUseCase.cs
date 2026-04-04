using DevHub.Application.DTOs;
using DevHub.Domain.Models;

namespace DevHub.Application.Interfaces;

public interface IAddProjectUseCase
{
    Task<Project> ExecuteAsync(CreateProjectRequest request, CancellationToken ct = default);
}
