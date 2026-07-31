using DevHub.Application.DTOs;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Enums;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Application.Tests.UseCases;

public class GetAllProjectsUseCaseTests
{
    private readonly InMemoryProjectRepository _repo = new();
    private readonly GetAllProjectsUseCase _useCase;

    public GetAllProjectsUseCaseTests()
    {
        _useCase = new GetAllProjectsUseCase(_repo);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyRepository_ReturnsEmptyList()
    {
        var result = await _useCase.ExecuteAsync(null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithProjects_ReturnsAll()
    {
        var p1 = Domain.Models.Project.Create("A", "D:\\a", ProgrammingLanguage.CSharp);
        var p2 = Domain.Models.Project.Create("B", "D:\\b", ProgrammingLanguage.Python);
        await _repo.AddAsync(p1);
        await _repo.AddAsync(p2);

        var result = await _useCase.ExecuteAsync(null);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByStatus_ReturnsMatchingProjects()
    {
        var p1 = Domain.Models.Project.Create("Active", "D:\\a", ProgrammingLanguage.CSharp);
        var p2 = Domain.Models.Project.Create("Completed", "D:\\b", ProgrammingLanguage.Python);
        p2.ChangeStatus(ProjectStatus.Completed);
        await _repo.AddAsync(p1);
        await _repo.AddAsync(p2);

        var result = await _useCase.ExecuteAsync(new ProjectFilter(Status: ProjectStatus.Completed));

        Assert.Single(result);
        Assert.Equal("Completed", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FilterBySearchQuery_ReturnsMatchingProjects()
    {
        var p1 = Domain.Models.Project.Create("WebApp", "D:\\a", ProgrammingLanguage.CSharp);
        var p2 = Domain.Models.Project.Create("MobileApp", "D:\\b", ProgrammingLanguage.Python);
        await _repo.AddAsync(p1);
        await _repo.AddAsync(p2);

        var result = await _useCase.ExecuteAsync(new ProjectFilter(SearchQuery: "Web"));

        Assert.Single(result);
        Assert.Equal("WebApp", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_FilterBySearchQuery_CaseInsensitive()
    {
        var p1 = Domain.Models.Project.Create("WebApp", "D:\\a", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(p1);

        var result = await _useCase.ExecuteAsync(new ProjectFilter(SearchQuery: "webapp"));

        Assert.Single(result);
    }

    [Fact]
    public async Task ExecuteAsync_SearchDescription_FindsMatch()
    {
        var p1 = Domain.Models.Project.Create("App", "D:\\a", ProgrammingLanguage.CSharp);
        p1.UpdateDescription("A web application");
        await _repo.AddAsync(p1);

        var result = await _useCase.ExecuteAsync(new ProjectFilter(SearchQuery: "web"));

        Assert.Single(result);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByTags_ReturnsMatchingProjects()
    {
        var p1 = Domain.Models.Project.Create("A", "D:\\a", ProgrammingLanguage.CSharp);
        p1.SetTags(["api", "backend"]);
        var p2 = Domain.Models.Project.Create("B", "D:\\b", ProgrammingLanguage.Python);
        p2.SetTags(["frontend"]);
        await _repo.AddAsync(p1);
        await _repo.AddAsync(p2);

        var result = await _useCase.ExecuteAsync(new ProjectFilter(Tags: ["api"]));

        Assert.Single(result);
        Assert.Equal("A", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_HiddenProjects_ExcludedByDefault()
    {
        var p1 = Domain.Models.Project.Create("Visible", "D:\\a", ProgrammingLanguage.CSharp);
        var p2 = Domain.Models.Project.Create("Hidden", "D:\\b", ProgrammingLanguage.Python);
        p2.ToggleHidden();
        await _repo.AddAsync(p1);
        await _repo.AddAsync(p2);

        var result = await _useCase.ExecuteAsync(new ProjectFilter(ShowHidden: false));

        Assert.Single(result);
        Assert.Equal("Visible", result[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_HiddenProjects_IncludedWhenShowHiddenTrue()
    {
        var p1 = Domain.Models.Project.Create("Visible", "D:\\a", ProgrammingLanguage.CSharp);
        var p2 = Domain.Models.Project.Create("Hidden", "D:\\b", ProgrammingLanguage.Python);
        p2.ToggleHidden();
        await _repo.AddAsync(p1);
        await _repo.AddAsync(p2);

        var result = await _useCase.ExecuteAsync(new ProjectFilter(ShowHidden: true));

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_OrderByFavoriteFirst()
    {
        var p1 = Domain.Models.Project.Create("NotFav", "D:\\a", ProgrammingLanguage.CSharp);
        var p2 = Domain.Models.Project.Create("Fav", "D:\\b", ProgrammingLanguage.Python);
        p2.ToggleFavorite();
        await _repo.AddAsync(p1);
        await _repo.AddAsync(p2);

        var result = await _useCase.ExecuteAsync(null);

        Assert.Equal("Fav", result[0].Name);
        Assert.Equal("NotFav", result[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsProjectDto()
    {
        var p = Domain.Models.Project.Create("Test", "D:\\test", ProgrammingLanguage.CSharp);
        p.UpdateDescription("desc");
        await _repo.AddAsync(p);

        var result = await _useCase.ExecuteAsync(null);

        Assert.Single(result);
        Assert.Equal(p.Id, result[0].Id);
        Assert.Equal("Test", result[0].Name);
        Assert.Equal("D:\\test", result[0].Path);
        Assert.Equal("desc", result[0].Description);
        Assert.Equal(ProgrammingLanguage.CSharp, result[0].Language);
    }
}
