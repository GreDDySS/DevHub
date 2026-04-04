using DevHub.Application.UseCases.Links;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Integration.Tests;

public class CaptureLinkUseCaseTests
{
    private class MockClipboardService(string? text) : IClipboardService
    {
        public Task<string?> GetTextAsync(CancellationToken ct = default) => Task.FromResult(text);
        public Task SetTextAsync(string text, CancellationToken ct = default) => Task.CompletedTask;
    }

    private static CaptureLinkUseCase CreateUseCase(InMemoryLinkRepository repo, MockClipboardService clipboard)
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        return new CaptureLinkUseCase(repo, clipboard, httpClient);
    }

    [Fact]
    public async Task ExecuteAsync_ValidGitHubUrl_ReturnsRepositoryLink()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://github.com/user/repo");
        var useCase = CreateUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync(null);

        Assert.NotNull(result);
        Assert.Equal("https://github.com/user/repo", result.Url);
        Assert.Equal(LinkType.Repository, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_YouTubeUrl_ReturnsYouTubeLink()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://youtube.com/watch?v=123");
        var useCase = CreateUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync(null);

        Assert.NotNull(result);
        Assert.Equal(LinkType.YouTube, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_ArticleUrl_ReturnsArticleLink()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://blog.example.com/post");
        var useCase = CreateUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync(null);

        Assert.NotNull(result);
        Assert.Equal(LinkType.Article, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_DocsUrl_ReturnsDocumentationLink()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://docs.example.com/api");
        var useCase = CreateUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync(null);

        Assert.NotNull(result);
        Assert.Equal(LinkType.Documentation, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidUrl_ReturnsNull()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("not a url");
        var useCase = CreateUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync(null);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyClipboard_ReturnsNull()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService(null);
        var useCase = CreateUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync(null);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceClipboard_ReturnsNull()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("   ");
        var useCase = CreateUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync(null);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithProjectId_LinksToProject()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://example.com");
        var useCase = CreateUseCase(repo, clipboard);
        var projectId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(projectId);

        Assert.NotNull(result);
        Assert.Equal(projectId, result.ProjectId);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutProjectId_ProjectIdIsNull()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://example.com");
        var useCase = CreateUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync(null);

        Assert.NotNull(result);
        Assert.Null(result.ProjectId);
    }

    [Fact]
    public async Task ExecuteAsync_SavesToRepository()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://example.com");
        var useCase = CreateUseCase(repo, clipboard);

        await useCase.ExecuteAsync(null);

        var all = await repo.GetAllAsync();
        Assert.Single(all);
        Assert.Equal("https://example.com", all[0].Url);
    }
}
