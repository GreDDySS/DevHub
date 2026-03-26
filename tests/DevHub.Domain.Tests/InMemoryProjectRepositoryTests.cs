using DevHub.Domain.Enums;
using DevHub.Domain.Models;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Domain.Tests;

public class InMemoryProjectRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddProject()
    {
        var repo = new InMemoryProjectRepository();
        var project = new Project { Name = "Test", Path = "D:\\Test", Language = ProgrammingLanguage.CSharp };

        await repo.AddAsync(project);
        var all = await repo.GetAllAsync();

        Assert.Single(all);
        Assert.Equal("Test", all[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProject()
    {
        var repo = new InMemoryProjectRepository();
        var project = new Project { Name = "Test", Path = "D:\\Test" };

        await repo.AddAsync(project);
        var found = await repo.GetByIdAsync(project.Id);

        Assert.NotNull(found);
        Assert.Equal(project.Id, found.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var repo = new InMemoryProjectRepository();

        var found = await repo.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProject()
    {
        var repo = new InMemoryProjectRepository();
        var project = new Project { Name = "Old", Path = "D:\\Test" };

        await repo.AddAsync(project);
        project.Name = "New";
        await repo.UpdateAsync(project);

        var found = await repo.GetByIdAsync(project.Id);
        Assert.Equal("New", found!.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProject()
    {
        var repo = new InMemoryProjectRepository();
        var project = new Project { Name = "Test", Path = "D:\\Test" };

        await repo.AddAsync(project);
        await repo.DeleteAsync(project.Id);

        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoProjects()
    {
        var repo = new InMemoryProjectRepository();

        var all = await repo.GetAllAsync();

        Assert.Empty(all);
    }
}
