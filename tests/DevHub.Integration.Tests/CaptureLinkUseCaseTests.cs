using DevHub.Application.UseCases.Links;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Integration.Tests;

public class CaptureLinkUseCaseTests
{
    private class MockClipboardService : IClipboardService
    {
        private readonly string? _text;
        public MockClipboardService(string? text) => _text = text;
        public Task<string?> GetTextAsync() => Task.FromResult(_text);
        public Task SetTextAsync(string text) => Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_ValidGitHubUrl_ReturnsRepositoryLink()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://github.com/user/repo");
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync();

        Assert.NotNull(result);
        Assert.Equal("https://github.com/user/repo", result.Url);
        Assert.Equal(LinkType.Repository, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_YouTubeUrl_ReturnsYouTubeLink()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://youtube.com/watch?v=123");
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync();

        Assert.NotNull(result);
        Assert.Equal(LinkType.YouTube, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_ArticleUrl_ReturnsArticleLink()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://blog.example.com/post");
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync();

        Assert.NotNull(result);
        Assert.Equal(LinkType.Article, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_DocsUrl_ReturnsDocumentationLink()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://docs.example.com/api");
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync();

        Assert.NotNull(result);
        Assert.Equal(LinkType.Documentation, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidUrl_ReturnsNull()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("not a url");
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyClipboard_ReturnsNull()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService(null);
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceClipboard_ReturnsNull()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("   ");
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithProjectId_LinksToProject()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://example.com");
        var useCase = new CaptureLinkUseCase(repo, clipboard);
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
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        var result = await useCase.ExecuteAsync();

        Assert.NotNull(result);
        Assert.Null(result.ProjectId);
    }

    [Fact]
    public async Task ExecuteAsync_SavesToRepository()
    {
        var repo = new InMemoryLinkRepository();
        var clipboard = new MockClipboardService("https://example.com");
        var useCase = new CaptureLinkUseCase(repo, clipboard);

        await useCase.ExecuteAsync();

        var all = await repo.GetAllAsync();
        Assert.Single(all);
        Assert.Equal("https://example.com", all[0].Url);
    }
}
