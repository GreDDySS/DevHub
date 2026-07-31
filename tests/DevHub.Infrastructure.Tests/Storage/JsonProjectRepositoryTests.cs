using DevHub.Domain.Enums;
using DevHub.Domain.Models;
using DevHub.Infrastructure.Storage;

namespace DevHub.Infrastructure.Tests.Storage;

public class JsonProjectRepositoryTests : IDisposable
{
    private readonly string _tempFile;
    private readonly JsonProjectRepository _repo;

    public JsonProjectRepositoryTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), "DevHub_Test_" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(_tempFile, "{\"version\":1,\"items\":[]}");
        _repo = new JsonProjectRepository(_tempFile);
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
    public async Task AddAsync_StoresProject()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);

        await _repo.AddAsync(project);

        var result = await _repo.GetAllAsync();
        Assert.Single(result);
        Assert.Equal("Test", result[0].Name);
    }

    [Fact]
    public async Task AddAsync_MultipleProjects_StoresAll()
    {
        var p1 = Project.Create("A", "D:\\a", ProgrammingLanguage.CSharp);
        var p2 = Project.Create("B", "D:\\b", ProgrammingLanguage.Python);

        await _repo.AddAsync(p1);
        await _repo.AddAsync(p2);

        var result = await _repo.GetAllAsync();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProject_ReturnsProject()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        var result = await _repo.GetByIdAsync(default, project.Id);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProject_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(default, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProject_UpdatesInPlace()
    {
        var project = Project.Create("Original", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        project.Rename("Updated");
        await _repo.UpdateAsync(project);

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProject_DoesNotThrow()
    {
        var project = Project.Create("Ghost", "D:\\path", ProgrammingLanguage.CSharp);

        await _repo.UpdateAsync(project);

        var all = await _repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProject_RemovesIt()
    {
        var project = Project.Create("ToDelete", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _repo.DeleteAsync(project.Id);

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProject_DoesNotThrow()
    {
        await _repo.DeleteAsync(Guid.NewGuid());

        var all = await _repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task AddAndRead_DataPersists()
    {
        var project = Project.Create("Persistent", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        // Create a new repo instance with the same file
        var repo2 = new JsonProjectRepository(_tempFile);
        var result = await repo2.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Persistent", result[0].Name);
    }

    [Fact]
    public async Task AddAsync_ProjectWithTags_PreservesTags()
    {
        var project = Project.Create("Tagged", "D:\\path", ProgrammingLanguage.CSharp);
        project.SetTags(["api", "web"]);
        await _repo.AddAsync(project);

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal(2, result.Tags.Count);
        Assert.Contains("api", result.Tags);
    }

    [Fact]
    public async Task CorruptedFile_ReturnsEmptyList()
    {
        await File.WriteAllTextAsync(_tempFile, "{ invalid json }");

        var result = await _repo.GetAllAsync();

        Assert.Empty(result);
    }
}
