using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public class JsonLinkRepository(string filePath = null!) : JsonFileStore<Link>(filePath ?? AppPaths.LinksFile), ILinkRepository
{
    public async Task<List<Link>> GetAllAsync(CancellationToken ct = default)
        => await LoadAllAsync(ct);

    public async Task<Link?> GetByIdAsync(CancellationToken ct, Guid id)
    {
        var links = await LoadAllAsync(ct);
        return links.FirstOrDefault(l => l.Id == id);
    }

    public async Task<List<Link>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        var links = await LoadAllAsync(ct);
        return links.Where(l => l.ProjectId == projectId).ToList();
    }

    public async Task AddAsync(Link link, CancellationToken ct = default)
    {
        var links = await LoadAllAsync(ct);
        links.Add(link);
        await SaveAllAsync(links, ct);
    }

    public async Task UpdateAsync(Link link, CancellationToken ct = default)
    {
        var links = await LoadAllAsync(ct);
        var index = links.FindIndex(l => l.Id == link.Id);
        if (index >= 0)
        {
            links[index] = link;
            await SaveAllAsync(links, ct);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var links = await LoadAllAsync(ct);
        links.RemoveAll(l => l.Id == id);
        await SaveAllAsync(links, ct);
    }
}
