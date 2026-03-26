using DevHub.Domain.Models;

namespace DevHub.Domain.Interfaces;

public interface ILinkRepository
{
    Task<List<Link>> GetAllAsync();
    Task<Link?> GetByIdAsync(Guid id);
    Task<List<Link>> GetByProjectIdAsync(Guid projectId);
    Task AddAsync(Link link);
    Task UpdateAsync(Link link);
    Task DeleteAsync(Guid id);
}
