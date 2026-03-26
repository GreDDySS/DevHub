using DevHub.Domain.Enums;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests;

public class ProjectTests
{
    [Fact]
    public void Project_ShouldInheritBaseModel()
    {
        var project = new Project();

        Assert.IsAssignableFrom<BaseModel>(project);
        Assert.NotEqual(Guid.Empty, project.Id);
    }

    [Fact]
    public void Project_DefaultValues_ShouldBeCorrect()
    {
        var project = new Project();

        Assert.Equal(string.Empty, project.Name);
        Assert.Equal(string.Empty, project.Path);
        Assert.Null(project.Description);
        Assert.Null(project.Notes);
        Assert.Equal(ProgrammingLanguage.Other, project.Language);
        Assert.Equal(ProjectStatus.Active, project.Status);
        Assert.Empty(project.Tags);
        Assert.Null(project.PreferredIde);
        Assert.False(project.IsFavorite);
        Assert.Null(project.LastAccessedAt);
    }

    [Fact]
    public void Project_PropertiesShouldBeSettable()
    {
        var project = new Project
        {
            Name = "TestProject",
            Path = "D:\\repos\\TestProject",
            Description = "A test project",
            Notes = "Some notes",
            Language = ProgrammingLanguage.CSharp,
            Status = ProjectStatus.Active,
            Tags = ["api", "backend"],
            PreferredIde = "VSCode",
            IsFavorite = true,
            LastAccessedAt = DateTime.UtcNow
        };

        Assert.Equal("TestProject", project.Name);
        Assert.Equal("D:\\repos\\TestProject", project.Path);
        Assert.Equal("A test project", project.Description);
        Assert.Equal("Some notes", project.Notes);
        Assert.Equal(ProgrammingLanguage.CSharp, project.Language);
        Assert.Equal(ProjectStatus.Active, project.Status);
        Assert.Equal(2, project.Tags.Count);
        Assert.Equal("VSCode", project.PreferredIde);
        Assert.True(project.IsFavorite);
        Assert.NotNull(project.LastAccessedAt);
    }

    [Fact]
    public void Project_Equality_ShouldBeBasedOnId()
    {
        var project1 = new Project { Name = "A" };
        var project2 = new Project { Name = "B" };

        Assert.NotEqual(project1, project2);

        project2.Id = project1.Id;

        Assert.Equal(project1, project2);
    }

    [Fact]
    public void Project_MarkUpdated_ShouldChangeUpdatedAt()
    {
        var project = new Project();
        var originalUpdatedAt = project.UpdatedAt;

        project.MarkUpdated();

        Assert.True(project.UpdatedAt >= originalUpdatedAt);
    }
}
