using DevHub.Application.DTOs;
using DevHub.Application.Exceptions;
using DevHub.Application.UseCases.Projects;
using DevHub.Domain.Enums;
using DevHub.Domain.Interfaces;
using DevHub.Domain.Models;
using DevHub.Domain.Tests.InMemory;

namespace DevHub.Application.Tests.UseCases;

public class UpdateProjectUseCaseTests
{
    private readonly InMemoryProjectRepository _repo = new();
    private readonly UpdateProjectUseCase _useCase;

    public UpdateProjectUseCaseTests()
    {
        _useCase = new UpdateProjectUseCase(_repo);
    }

    [Fact]
    public async Task ExecuteAsync_ValidUpdate_UpdatesProject()
    {
        var project = Project.Create("Original", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(Name: "Updated"));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ProjectNotFound_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _useCase.ExecuteAsync(Guid.NewGuid(), new UpdateProjectRequest(Name: "X")));
    }

    [Fact]
    public async Task ExecuteAsync_UpdateDescription_UpdatesDescription()
    {
        var project = Project.Create("P", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(Description: "new desc"));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal("new desc", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateNotes_UpdatesNotes()
    {
        var project = Project.Create("P", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(Notes: "new notes"));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal("new notes", result.Notes);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateStatus_UpdatesStatus()
    {
        var project = Project.Create("P", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(Status: ProjectStatus.Completed));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal(ProjectStatus.Completed, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateLanguage_UpdatesLanguage()
    {
        var project = Project.Create("P", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(Language: ProgrammingLanguage.Python));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal(ProgrammingLanguage.Python, result.Language);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateTags_UpdatesTags()
    {
        var project = Project.Create("P", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(Tags: ["api", "web"]));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal(2, result.Tags.Count);
        Assert.Contains("api", result.Tags);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatePreferredIde_UpdatesPreferredIde()
    {
        var project = Project.Create("P", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(PreferredIde: "C:\\VS\\code.exe"));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal("C:\\VS\\code.exe", result.PreferredIde);
    }

    [Fact]
    public async Task ExecuteAsync_ToggleFavorite_TogglesFavorite()
    {
        var project = Project.Create("P", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(IsFavorite: true));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.True(result.IsFavorite);
    }

    [Fact]
    public async Task ExecuteAsync_ToggleHidden_TogglesHidden()
    {
        var project = Project.Create("P", "D:\\path", ProgrammingLanguage.CSharp);
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest(IsHidden: true));

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.True(result.IsHidden);
    }

    [Fact]
    public async Task ExecuteAsync_NullFields_DoesNotChangeExisting()
    {
        var project = Project.Create("Original", "D:\\path", ProgrammingLanguage.CSharp);
        project.UpdateDescription("original desc");
        await _repo.AddAsync(project);

        await _useCase.ExecuteAsync(project.Id, new UpdateProjectRequest());

        var result = await _repo.GetByIdAsync(default, project.Id);
        Assert.Equal("Original", result.Name);
        Assert.Equal("original desc", result.Description);
    }
}
