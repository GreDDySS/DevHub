using DevHub.Domain.Models;

namespace DevHub.Domain.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(CancellationToken ct, Guid id);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
