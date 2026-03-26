using DevHub.Application.DTOs;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Enums;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Integration.Tests;

public class GetAllProjectsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyRepo_ReturnsEmpty()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        var result = await useCase.ExecuteAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAllProjects()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        await repo.AddAsync(new Domain.Models.Project { Name = "A", Path = "D:\\A" });
        await repo.AddAsync(new Domain.Models.Project { Name = "B", Path = "D:\\B" });

        var result = await useCase.ExecuteAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByStatus()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        await repo.AddAsync(new Domain.Models.Project { Name = "A", Path = "D:\\A", Status = ProjectStatus.Active });
        await repo.AddAsync(new Domain.Models.Project { Name = "B", Path = "D:\\B", Status = ProjectStatus.Archived });

        var result = await useCase.ExecuteAsync(new ProjectFilter(Status: ProjectStatus.Active));

        Assert.Single(result);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FilterBySearch()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        await repo.AddAsync(new Domain.Models.Project { Name = "WebApp", Path = "D:\\WebApp" });
        await repo.AddAsync(new Domain.Models.Project { Name = "DesktopApp", Path = "D:\\Desktop" });

        var result = await useCase.ExecuteAsync(new ProjectFilter(SearchQuery: "Web"));

        Assert.Single(result);
        Assert.Equal("WebApp", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FavoritesFirst()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        await repo.AddAsync(new Domain.Models.Project { Name = "A", Path = "D\\A", IsFavorite = false });
        await repo.AddAsync(new Domain.Models.Project { Name = "B", Path = "D\\B", IsFavorite = true });

        var result = await useCase.ExecuteAsync();

        Assert.Equal("B", result[0].Name);
    }
}
