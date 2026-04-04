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
        var link = Link.Create("https://example.com", LinkType.Article);

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

        var link1 = Link.Create("https://a.com");
        link1.SetProjectId(projectId);
        var link2 = Link.Create("https://b.com");
        link2.SetProjectId(projectId);
        var link3 = Link.Create("https://c.com");

        await repo.AddAsync(link1);
        await repo.AddAsync(link2);
        await repo.AddAsync(link3);

        var links = await repo.GetByProjectIdAsync(projectId);

        Assert.Equal(2, links.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateLink()
    {
        var repo = new InMemoryLinkRepository();
        var link = Link.Create("https://old.com");

        await repo.AddAsync(link);
        link.SetTitle("Updated");
        await repo.UpdateAsync(link);

        var found = await repo.GetByIdAsync(default, link.Id);
        Assert.Equal("Updated", found!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLink()
    {
        var repo = new InMemoryLinkRepository();
        var link = Link.Create("https://example.com");

        await repo.AddAsync(link);
        var found = await repo.GetByIdAsync(default, link.Id);

        Assert.NotNull(found);
        Assert.Equal(link.Id, found.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var repo = new InMemoryLinkRepository();

        var found = await repo.GetByIdAsync(default, Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveLink()
    {
        var repo = new InMemoryLinkRepository();
        var link = Link.Create("https://example.com");

        await repo.AddAsync(link);
        await repo.DeleteAsync(link.Id);

        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }
}
