using DevHub.Domain.Enums;
using DevHub.Domain.Models;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Domain.Tests;

public class InMemoryLinkRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddLink()
    {
        var repo = new InMemoryLinkRepository();
        var link = new Link { Url = "https://example.com", Type = LinkType.Article };

        await repo.AddAsync(link);
        var all = await repo.GetAllAsync();

        Assert.Single(all);
        Assert.Equal("https://example.com", all[0].Url);
    }

    [Fact]
    public async Task GetByProjectIdAsync_ShouldReturnLinks()
    {
        var repo = new InMemoryLinkRepository();
        var projectId = Guid.NewGuid();

        await repo.AddAsync(new Link { Url = "https://a.com", ProjectId = projectId });
        await repo.AddAsync(new Link { Url = "https://b.com", ProjectId = projectId });
        await repo.AddAsync(new Link { Url = "https://c.com" });

        var links = await repo.GetByProjectIdAsync(projectId);

        Assert.Equal(2, links.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateLink()
    {
        var repo = new InMemoryLinkRepository();
        var link = new Link { Url = "https://old.com" };

        await repo.AddAsync(link);
        link.Url = "https://new.com";
        await repo.UpdateAsync(link);

        var found = await repo.GetByIdAsync(link.Id);
        Assert.Equal("https://new.com", found!.Url);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveLink()
    {
        var repo = new InMemoryLinkRepository();
        var link = new Link { Url = "https://example.com" };

        await repo.AddAsync(link);
        await repo.DeleteAsync(link.Id);

        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }
}
