using DevHub.Domain.Enums;

namespace DevHub.Domain.Tests;

public class EnumTests
{
    [Fact]
    public void ProjectStatus_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues<ProjectStatus>();

        Assert.Contains(ProjectStatus.Active, values);
        Assert.Contains(ProjectStatus.Completed, values);
        Assert.Contains(ProjectStatus.Paused, values);
        Assert.Contains(ProjectStatus.Archived, values);
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void ProgrammingLanguage_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues<ProgrammingLanguage>();

        Assert.Contains(ProgrammingLanguage.CSharp, values);
        Assert.Contains(ProgrammingLanguage.Python, values);
        Assert.Contains(ProgrammingLanguage.Rust, values);
        Assert.Contains(ProgrammingLanguage.JavaScript, values);
        Assert.Contains(ProgrammingLanguage.TypeScript, values);
        Assert.Contains(ProgrammingLanguage.Go, values);
        Assert.Contains(ProgrammingLanguage.Java, values);
        Assert.Contains(ProgrammingLanguage.Cpp, values);
        Assert.Contains(ProgrammingLanguage.Other, values);
        Assert.Equal(9, values.Length);
    }

    [Fact]
    public void LinkType_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues<LinkType>();

        Assert.Contains(LinkType.YouTube, values);
        Assert.Contains(LinkType.Article, values);
        Assert.Contains(LinkType.Repository, values);
        Assert.Contains(LinkType.Documentation, values);
        Assert.Contains(LinkType.Other, values);
        Assert.Equal(5, values.Length);
    }
}
