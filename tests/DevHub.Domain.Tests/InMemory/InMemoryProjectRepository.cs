using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests.InMemory;

public class InMemoryProjectRepository : IProjectRepository
{
    private readonly List<Project> _projects = [];

    public Task<List<Project>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult(_projects.ToList());

    public Task<Project?> GetByIdAsync(CancellationToken ct, Guid id)
        => Task.FromResult(_projects.FirstOrDefault(p => p.Id == id));

    public Task AddAsync(Project project, CancellationToken ct = default)
    {
        _projects.Add(project);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        var index = _projects.FindIndex(p => p.Id == project.Id);
        if (index >= 0) _projects[index] = project;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _projects.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
