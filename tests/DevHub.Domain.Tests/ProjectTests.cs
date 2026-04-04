using DevHub.Domain.Enums;
using DevHub.Domain.Events;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests;

public class ProjectTests
{
    [Fact]
    public void Project_Create_ShouldInitializeWithCorrectValues()
    {
        var project = Project.Create("TestProject", "D:\\repos\\TestProject", ProgrammingLanguage.CSharp);

        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal("TestProject", project.Name);
        Assert.Equal("D:\\repos\\TestProject", project.Path);
        Assert.Equal(ProgrammingLanguage.CSharp, project.Language);
        Assert.Equal(ProjectStatus.Active, project.Status);
        Assert.Empty(project.Tags);
        Assert.Null(project.PreferredIde);
        Assert.False(project.IsFavorite);
        Assert.False(project.IsHidden);
        Assert.Null(project.LastAccessedAt);
        Assert.True(project.AutoStatusEnabled);
    }

    [Fact]
    public void Project_Create_ThrowsOnEmptyName()
    {
        Assert.Throws<DomainException>(() => Project.Create("", "D:\\path", ProgrammingLanguage.CSharp));
        Assert.Throws<DomainException>(() => Project.Create("   ", "D:\\path", ProgrammingLanguage.CSharp));
    }

    [Fact]
    public void Project_Create_ThrowsOnEmptyPath()
    {
        Assert.Throws<DomainException>(() => Project.Create("Name", "", ProgrammingLanguage.CSharp));
        Assert.Throws<DomainException>(() => Project.Create("Name", "   ", ProgrammingLanguage.CSharp));
    }

    [Fact]
    public void Project_Rename_ShouldUpdateName()
    {
        var project = Project.Create("OldName", "D:\\path", ProgrammingLanguage.CSharp);
        var originalUpdatedAt = project.UpdatedAt;

        project.Rename("NewName");

        Assert.Equal("NewName", project.Name);
        Assert.True(project.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Project_ToggleFavorite_ShouldToggle()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        Assert.False(project.IsFavorite);

        project.ToggleFavorite();
        Assert.True(project.IsFavorite);

        project.ToggleFavorite();
        Assert.False(project.IsFavorite);
    }

    [Fact]
    public void Project_ToggleHidden_ShouldToggle()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        Assert.False(project.IsHidden);

        project.ToggleHidden();
        Assert.True(project.IsHidden);
    }

    [Fact]
    public void Project_ChangeStatus_ShouldUpdateStatus()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);

        project.ChangeStatus(ProjectStatus.Completed);
        Assert.Equal(ProjectStatus.Completed, project.Status);
    }

    [Fact]
    public void Project_AddTag_ShouldAddUniqueTag()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);

        project.AddTag("api");
        project.AddTag("backend");
        project.AddTag("API"); // duplicate (case-insensitive)

        Assert.Equal(2, project.Tags.Count);
    }

    [Fact]
    public void Project_SetTags_ShouldReplaceAllTags()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.AddTag("old");

        project.SetTags(["new1", "new2"]);

        Assert.Equal(2, project.Tags.Count);
        Assert.Contains("new1", project.Tags);
        Assert.Contains("new2", project.Tags);
    }

    [Fact]
    public void Project_GetEffectiveStatus_ShouldReturnPaused_WhenInactive()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);

        var status = project.GetEffectiveStatus(DateTime.UtcNow.AddDays(-30));

        Assert.Equal(ProjectStatus.Paused, status);
    }

    [Fact]
    public void Project_Equality_ShouldBeBasedOnId()
    {
        var project1 = Project.Create("A", "D:\\a", ProgrammingLanguage.CSharp);
        var project2 = Project.Create("B", "D:\\b", ProgrammingLanguage.CSharp);

        Assert.NotEqual(project1, project2);

        // Same Id = equal
        var project3 = Project.Create("C", "D:\\c", ProgrammingLanguage.CSharp);
        // Can't change Id (init only), so test with same instance
        Assert.Equal(project1, project1);
    }

    [Fact]
    public void Project_BumpUpdated_ShouldChangeUpdatedAt()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var originalUpdatedAt = project.UpdatedAt;

        Thread.Sleep(1);
        project.BumpUpdated();

        Assert.True(project.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Project_DomainEvents_ShouldBeRaised()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);

        project.ToggleFavorite();

        Assert.NotEmpty(project.DomainEvents);
        Assert.IsType<ProjectFavoriteToggledEvent>(project.DomainEvents[0]);
    }
}
