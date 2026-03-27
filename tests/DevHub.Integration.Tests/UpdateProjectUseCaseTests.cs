using DevHub.Application.DTOs;
using DevHub.Application.Exceptions;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Integration.Tests;

public class UpdateProjectUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdateName()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new UpdateProjectUseCase(repo);

        var project = new Domain.Models.Project { Name = "Old", Path = "D:\\Test" };
        await repo.AddAsync(project);

        await useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(Name: "New"));

        var updated = await repo.GetByIdAsync(project.Id);
        Assert.Equal("New", updated!.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ProjectNotFound_Throws()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new UpdateProjectUseCase(repo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.ExecuteAsync(Guid.NewGuid(), new UpdateProjectRequest(Name: "New")));
    }

    [Fact]
    public async Task ExecuteAsync_NullFieldsNotUpdated()
    {
        var repo = new InMemoryProjectRepository();
        var useCase = new UpdateProjectUseCase(repo);

        var project = new Domain.Models.Project { Name = "Original", Path = "D:\\Test", Description = "Desc" };
        await repo.AddAsync(project);

        await useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(Name: "Updated"));

        var updated = await repo.GetByIdAsync(project.Id);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal("Desc", updated.Description);
    }
}
