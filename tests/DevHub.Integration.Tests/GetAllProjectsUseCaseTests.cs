using DevHub.Application.DTOs;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Enums;
using DevHub.Domain.Models;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Integration.Tests;

public class GetAllProjectsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyRepo_ReturnsEmpty()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        var result = await useCase.ExecuteAsync(null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAllProjects()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        await repo.AddAsync(Project.Create("A", "D:\\A", ProgrammingLanguage.Other));
        await repo.AddAsync(Project.Create("B", "D:\\B", ProgrammingLanguage.Other));

        var result = await useCase.ExecuteAsync(null);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByStatus()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        var a = Project.Create("A", "D:\\A", ProgrammingLanguage.Other);
        a.ChangeStatus(ProjectStatus.Active);
        await repo.AddAsync(a);

        var b = Project.Create("B", "D:\\B", ProgrammingLanguage.Other);
        b.ChangeStatus(ProjectStatus.Archived);
        await repo.AddAsync(b);

        var result = await useCase.ExecuteAsync(new ProjectFilter(Status: ProjectStatus.Active));

        Assert.Single(result);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FilterBySearch()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        await repo.AddAsync(Project.Create("WebApp", "D:\\WebApp", ProgrammingLanguage.Other));
        await repo.AddAsync(Project.Create("DesktopApp", "D:\\Desktop", ProgrammingLanguage.Other));

        var result = await useCase.ExecuteAsync(new ProjectFilter(SearchQuery: "Web"));

        Assert.Single(result);
        Assert.Equal("WebApp", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FavoritesFirst()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new GetAllProjectsUseCase(repo);

        var a = Project.Create("A", "D\\A", ProgrammingLanguage.Other);
        await repo.AddAsync(a);

        var b = Project.Create("B", "D\\B", ProgrammingLanguage.Other);
        b.ToggleFavorite();
        await repo.AddAsync(b);

        var result = await useCase.ExecuteAsync(null);

        Assert.Equal("B", result[0].Name);
    }
}
