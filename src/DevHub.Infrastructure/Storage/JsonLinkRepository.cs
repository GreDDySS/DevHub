using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Infrastructure.Configuration;

namespace DevHub.Infrastructure.Storage;

public class JsonLinkRepository : JsonFileStore<Link>, ILinkRepository
{
    public JsonLinkRepository() : base(AppPaths.LinksFile)
    {
    }

    public async Task<List<Link>> GetAllAsync()
        => await LoadAllAsync();

    public async Task<Link?> GetByIdAsync(Guid id)
    {
        var links = await LoadAllAsync();
        return links.FirstOrDefault(l => l.Id == id);
    }

    public async Task<List<Link>> GetByProjectIdAsync(Guid projectId)
    {
        var links = await LoadAllAsync();
        return links.Where(l => l.ProjectId == projectId).ToList();
    }

    public async Task AddAsync(Link link)
    {
        var links = await LoadAllAsync();
        links.Add(link);
        await SaveAllAsync(links);
    }

    public async Task UpdateAsync(Link link)
    {
        var links = await LoadAllAsync();
        var index = links.FindIndex(l => l.Id == link.Id);
        if (index >= 0)
        {
            links[index] = link;
            await SaveAllAsync(links);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var links = await LoadAllAsync();
        links.RemoveAll(l => l.Id == id);
        await SaveAllAsync(links);
    }
}
