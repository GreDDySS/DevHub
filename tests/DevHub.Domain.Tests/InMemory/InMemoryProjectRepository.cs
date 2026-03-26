using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests.InMemory;

public class InMemoryProjectRepository : IProjectRepository
{
    private readonly List<Project> _projects = [];

    public Task<List<Project>> GetAllAsync()
        => Task.FromResult(_projects.ToList());

    public Task<Project?> GetByIdAsync(Guid id)
        => Task.FromResult(_projects.FirstOrDefault(p => p.Id == id));

    public Task AddAsync(Project project)
    {
        _projects.Add(project);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Project project)
    {
        var index = _projects.FindIndex(p => p.Id == project.Id);
        if (index >= 0)
            _projects[index] = project;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _projects.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
