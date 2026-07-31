using DevHub.Domain.Enums;
using DevHub.Domain.Events;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests;

public class ProjectExtendedTests
{
    [Fact]
    public void Project_Create_ThrowsOnNameExceedingMaxLength()
    {
        var longName = new string('a', 201);
        Assert.Throws<DomainException>(() => Project.Create(longName, "D:\\path", ProgrammingLanguage.CSharp));
    }

    [Fact]
    public void Project_Create_AllowsNameAtMaxLength()
    {
        var maxName = new string('a', 200);
        var project = Project.Create(maxName, "D:\\path", ProgrammingLanguage.CSharp);
        Assert.Equal(maxName, project.Name);
    }

    [Fact]
    public void Project_Rename_ThrowsOnEmpty()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        Assert.Throws<DomainException>(() => project.Rename(""));
        Assert.Throws<DomainException>(() => project.Rename("   "));
    }

    [Fact]
    public void Project_Rename_ThrowsOnMaxLengthExceeded()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var longName = new string('a', 201);
        Assert.Throws<DomainException>(() => project.Rename(longName));
    }

    [Fact]
    public void Project_UpdateDescription_UpdatesValue()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.UpdateDescription("new description");
        Assert.Equal("new description", project.Description);
    }

    [Fact]
    public void Project_UpdateDescription_TrimsWhitespace()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.UpdateDescription("  trimmed  ");
        Assert.Equal("trimmed", project.Description);
    }

    [Fact]
    public void Project_UpdateDescription_ThrowsOnMaxLengthExceeded()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var longDesc = new string('a', 1001);
        Assert.Throws<DomainException>(() => project.UpdateDescription(longDesc));
    }

    [Fact]
    public void Project_UpdateDescription_AllowsNull()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.UpdateDescription("desc");
        project.UpdateDescription(null);
        Assert.Null(project.Description);
    }

    [Fact]
    public void Project_UpdateNotes_UpdatesValue()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.UpdateNotes("some notes");
        Assert.Equal("some notes", project.Notes);
    }

    [Fact]
    public void Project_UpdateNotes_TrimsWhitespace()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.UpdateNotes("  trimmed  ");
        Assert.Equal("trimmed", project.Notes);
    }

    [Fact]
    public void Project_UpdateNotes_ThrowsOnMaxLengthExceeded()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var longNotes = new string('a', 5001);
        Assert.Throws<DomainException>(() => project.UpdateNotes(longNotes));
    }

    [Fact]
    public void Project_UpdateNotes_AllowsNull()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.UpdateNotes("notes");
        project.UpdateNotes(null);
        Assert.Null(project.Notes);
    }

    [Fact]
    public void Project_ChangeLanguage_UpdatesLanguage()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.ChangeLanguage(ProgrammingLanguage.Python);
        Assert.Equal(ProgrammingLanguage.Python, project.Language);
    }

    [Fact]
    public void Project_ChangeLanguage_BumpsUpdated()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var before = project.UpdatedAt;
        Thread.Sleep(1);
        project.ChangeLanguage(ProgrammingLanguage.Python);
        Assert.True(project.UpdatedAt >= before);
    }

    [Fact]
    public void Project_SetPreferredIde_SetsValue()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.SetPreferredIde("C:\\VS\\code.exe");
        Assert.Equal("C:\\VS\\code.exe", project.PreferredIde);
    }

    [Fact]
    public void Project_SetPreferredIde_AllowsNull()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.SetPreferredIde("C:\\VS\\code.exe");
        project.SetPreferredIde(null);
        Assert.Null(project.PreferredIde);
    }

    [Fact]
    public void Project_RemoveTag_RemovesTag()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.AddTag("api");
        project.AddTag("web");
        project.RemoveTag("api");
        Assert.Single(project.Tags);
        Assert.Contains("web", project.Tags);
    }

    [Fact]
    public void Project_RemoveTag_CaseInsensitive()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.AddTag("API");
        project.RemoveTag("api");
        Assert.Empty(project.Tags);
    }

    [Fact]
    public void Project_RemoveTag_NonExistentTag_DoesNotThrow()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.RemoveTag("nonexistent");
        Assert.Empty(project.Tags);
    }

    [Fact]
    public void Project_MarkAccessed_SetsLastAccessedAt()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        Assert.Null(project.LastAccessedAt);

        project.MarkAccessed();

        Assert.NotNull(project.LastAccessedAt);
        Assert.True(project.LastAccessedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Project_EnableAutoStatus_SetsAutoStatusEnabled()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.DisableAutoStatus();
        Assert.False(project.AutoStatusEnabled);

        project.EnableAutoStatus();
        Assert.True(project.AutoStatusEnabled);
    }

    [Fact]
    public void Project_DisableAutoStatus_UnsetsAutoStatusEnabled()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        Assert.True(project.AutoStatusEnabled);

        project.DisableAutoStatus();
        Assert.False(project.AutoStatusEnabled);
    }

    [Fact]
    public void Project_GetEffectiveStatus_ReturnsActiveWhenRecent()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var status = project.GetEffectiveStatus(DateTime.UtcNow.AddDays(-5));
        Assert.Equal(ProjectStatus.Active, status);
    }

    [Fact]
    public void Project_GetEffectiveStatus_ReturnsStatusWhenAutoStatusDisabled()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.DisableAutoStatus();
        var status = project.GetEffectiveStatus(DateTime.UtcNow.AddDays(-30));
        Assert.Equal(ProjectStatus.Active, status);
    }

    [Fact]
    public void Project_GetEffectiveStatus_ReturnsStatusWhenNotActive()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.ChangeStatus(ProjectStatus.Completed);
        var status = project.GetEffectiveStatus(DateTime.UtcNow.AddDays(-30));
        Assert.Equal(ProjectStatus.Completed, status);
    }

    [Fact]
    public void Project_ToggleFavorite_AddsDomainEvent()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.ClearDomainEvents();

        project.ToggleFavorite();

        Assert.Single(project.DomainEvents);
        Assert.IsType<ProjectFavoriteToggledEvent>(project.DomainEvents[0]);
    }

    [Fact]
    public void Project_ToggleHidden_AddsDomainEvent()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.ClearDomainEvents();

        project.ToggleHidden();

        Assert.Single(project.DomainEvents);
        Assert.IsType<ProjectHiddenToggledEvent>(project.DomainEvents[0]);
    }

    [Fact]
    public void Project_ClearDomainEvents_EmptiesList()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.ToggleFavorite();
        project.ToggleHidden();

        project.ClearDomainEvents();

        Assert.Empty(project.DomainEvents);
    }

    [Fact]
    public void Project_AddTag_ThrowsOnEmpty()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        Assert.Throws<DomainException>(() => project.AddTag(""));
        Assert.Throws<DomainException>(() => project.AddTag("   "));
    }

    [Fact]
    public void Project_AddTag_ThrowsOnMaxLengthExceeded()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var longTag = new string('a', 51);
        Assert.Throws<DomainException>(() => project.AddTag(longTag));
    }

    [Fact]
    public void Project_AddTag_ThrowsOnMaxTagsCountExceeded()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        for (int i = 0; i < 50; i++)
            project.AddTag($"tag{i}");

        Assert.Throws<DomainException>(() => project.AddTag("oneMore"));
    }

    [Fact]
    public void Project_SetTags_NullArgument_Throws()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        Assert.Throws<ArgumentNullException>(() => project.SetTags(null!));
    }

    [Fact]
    public void Project_SetTags_FiltersEmptyAndTrims()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.SetTags(["  a  ", "", "  b  ", "   "]);
        Assert.Equal(2, project.Tags.Count);
        Assert.Contains("a", project.Tags);
        Assert.Contains("b", project.Tags);
    }

    [Fact]
    public void Project_SetTags_FiltersLongTags()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var longTag = new string('a', 51);
        project.SetTags(["short", longTag]);
        Assert.Single(project.Tags);
        Assert.Contains("short", project.Tags);
    }

    [Fact]
    public void Project_SetTags_DeduplicatesCaseInsensitive()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        project.SetTags(["API", "api", "Api"]);
        Assert.Single(project.Tags);
    }

    [Fact]
    public void Project_SetTags_LimitsToMaxCount()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var tags = Enumerable.Range(0, 60).Select(i => $"tag{i}").ToList();
        project.SetTags(tags);
        Assert.Equal(50, project.Tags.Count);
    }

    [Fact]
    public void Project_SetTags_BumpsUpdated()
    {
        var project = Project.Create("Test", "D:\\path", ProgrammingLanguage.CSharp);
        var before = project.UpdatedAt;
        Thread.Sleep(1);
        project.SetTags(["new"]);
        Assert.True(project.UpdatedAt >= before);
    }
}
