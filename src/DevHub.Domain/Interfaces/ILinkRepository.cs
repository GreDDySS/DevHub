using DevHub.Domain.Models;

namespace DevHub.Domain.Interfaces;

public interface ILinkRepository
{
    Task<List<Link>> GetAllAsync(CancellationToken ct = default);
    Task<Link?> GetByIdAsync(CancellationToken ct, Guid id);
    Task<List<Link>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task AddAsync(Link link, CancellationToken ct = default);
    Task UpdateAsync(Link link, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
