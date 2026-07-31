using DevHub.Domain.Enums;
using DevHub.Domain.Models;
using DevHub.Infrastructure.Storage;

namespace DevHub.Infrastructure.Tests.Storage;

public class JsonLinkRepositoryTests : IDisposable
{
    private readonly string _tempFile;
    private readonly JsonLinkRepository _repo;

    public JsonLinkRepositoryTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), "DevHub_Test_" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(_tempFile, "{\"version\":1,\"items\":[]}");
        _repo = new JsonLinkRepository(_tempFile);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public async Task GetAllAsync_EmptyFile_ReturnsEmptyList()
    {
        var result = await _repo.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_StoresLink()
    {
        var link = Link.Create("https://example.com", LinkType.Article);

        await _repo.AddAsync(link);

        var result = await _repo.GetAllAsync();
        Assert.Single(result);
        Assert.Equal("https://example.com", result[0].Url);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLink_ReturnsLink()
    {
        var link = Link.Create("https://example.com", LinkType.Article);
        await _repo.AddAsync(link);

        var result = await _repo.GetByIdAsync(default, link.Id);

        Assert.NotNull(result);
        Assert.Equal("https://example.com", result.Url);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingLink_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(default, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByProjectIdAsync_ReturnsLinksForProject()
    {
        var projectId = Guid.NewGuid();
        var link1 = Link.Create("https://a.com", LinkType.Article);
        link1.SetProjectId(projectId);
        var link2 = Link.Create("https://b.com", LinkType.Article);
        link2.SetProjectId(projectId);
        var link3 = Link.Create("https://c.com", LinkType.Article);

        await _repo.AddAsync(link1);
        await _repo.AddAsync(link2);
        await _repo.AddAsync(link3);

        var result = await _repo.GetByProjectIdAsync(projectId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByProjectIdAsync_NoLinks_ReturnsEmptyList()
    {
        var result = await _repo.GetByProjectIdAsync(Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingLink_UpdatesInPlace()
    {
        var link = Link.Create("https://original.com", LinkType.Article);
        await _repo.AddAsync(link);

        link.SetTitle("Updated Title");
        await _repo.UpdateAsync(link);

        var result = await _repo.GetByIdAsync(default, link.Id);
        Assert.Equal("Updated Title", result.Title);
    }

    [Fact]
    public async Task DeleteAsync_ExistingLink_RemovesIt()
    {
        var link = Link.Create("https://example.com", LinkType.Article);
        await _repo.AddAsync(link);

        await _repo.DeleteAsync(link.Id);

        var result = await _repo.GetByIdAsync(default, link.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAndRead_DataPersists()
    {
        var link = Link.Create("https://example.com", LinkType.YouTube);
        link.SetTitle("Persisted Link");
        await _repo.AddAsync(link);

        var repo2 = new JsonLinkRepository(_tempFile);
        var result = await repo2.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Persisted Link", result[0].Title);
    }

    [Fact]
    public async Task CorruptedFile_ReturnsEmptyList()
    {
        await File.WriteAllTextAsync(_tempFile, "{ broken json! }");

        var result = await _repo.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddMultiple_LinksWithDifferentProjects_FilteredByProject()
    {
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();

        var link1 = Link.Create("https://a.com", LinkType.Article);
        link1.SetProjectId(p1);
        var link2 = Link.Create("https://b.com", LinkType.Article);
        link2.SetProjectId(p2);
        var link3 = Link.Create("https://c.com", LinkType.Article);
        link3.SetProjectId(p1);

        await _repo.AddAsync(link1);
        await _repo.AddAsync(link2);
        await _repo.AddAsync(link3);

        var result1 = await _repo.GetByProjectIdAsync(p1);
        var result2 = await _repo.GetByProjectIdAsync(p2);

        Assert.Equal(2, result1.Count);
        Assert.Single(result2);
    }
}
