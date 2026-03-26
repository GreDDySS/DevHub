using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests.InMemory;

public class InMemoryLinkRepository : ILinkRepository
{
    private readonly List<Link> _links = [];

    public Task<List<Link>> GetAllAsync()
        => Task.FromResult(_links.ToList());

    public Task<Link?> GetByIdAsync(Guid id)
        => Task.FromResult(_links.FirstOrDefault(l => l.Id == id));

    public Task<List<Link>> GetByProjectIdAsync(Guid projectId)
        => Task.FromResult(_links.Where(l => l.ProjectId == projectId).ToList());

    public Task AddAsync(Link link)
    {
        _links.Add(link);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Link link)
    {
        var index = _links.FindIndex(l => l.Id == link.Id);
        if (index >= 0)
            _links[index] = link;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _links.RemoveAll(l => l.Id == id);
        return Task.CompletedTask;
    }
}
