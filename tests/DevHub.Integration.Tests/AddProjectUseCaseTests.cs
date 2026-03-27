using DevHub.Application.DTOs;
using DevHub.Application.Exceptions;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Enums;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Integration.Tests;

public class AddProjectUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsProject()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new AddProjectUseCase(repo);

        var result = await useCase.ExecuteAsync(new CreateProjectRequest("Test", "D:\\Test", "desc", ProgrammingLanguage.CSharp));

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal("D:\\Test", result.Path);
        Assert.Equal(ProjectStatus.Active, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyName_ThrowsValidationException()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new AddProjectUseCase(repo);

        await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecuteAsync(new CreateProjectRequest("", "D:\\Test", null, ProgrammingLanguage.CSharp)));
    }

    [Fact]
    public async Task ExecuteAsync_EmptyPath_ThrowsValidationException()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new AddProjectUseCase(repo);

        await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecuteAsync(new CreateProjectRequest("Test", "", null, ProgrammingLanguage.CSharp)));
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceOnly_ThrowsValidationException()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new AddProjectUseCase(repo);

        await Assert.ThrowsAsync<ValidationException>(() =>
            useCase.ExecuteAsync(new CreateProjectRequest("   ", "D:\\Test", null, ProgrammingLanguage.CSharp)));
    }

    [Fact]
    public async Task ExecuteAsync_TrimmedName()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new AddProjectUseCase(repo);

        var result = await useCase.ExecuteAsync(new CreateProjectRequest("  MyProject  ", "D:\\Test", null, ProgrammingLanguage.CSharp));

        Assert.Equal("MyProject", result.Name);
    }
}
