using DevHub.Domain.Models;

namespace DevHub.Domain.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(Guid id);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
}
