using DevHub.Domain.Enums;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests;

public class LinkTests
{
    [Fact]
    public void Link_ShouldInheritBaseModel()
    {
        var link = new Link();

        Assert.IsAssignableFrom<BaseModel>(link);
        Assert.NotEqual(Guid.Empty, link.Id);
    }

    [Fact]
    public void Link_DefaultValues_ShouldBeCorrect()
    {
        var link = new Link();

        Assert.Equal(string.Empty, link.Url);
        Assert.Null(link.Title);
        Assert.Equal(LinkType.Other, link.Type);
        Assert.Null(link.ProjectId);
        Assert.Empty(link.Tags);
        Assert.Null(link.Notes);
    }

    [Fact]
    public void Link_PropertiesShouldBeSettable()
    {
        var projectId = Guid.NewGuid();
        var link = new Link
        {
            Url = "https://github.com/test/repo",
            Title = "Test Repository",
            Type = LinkType.Repository,
            ProjectId = projectId,
            Tags = ["git", "open-source"],
            Notes = "Useful repo"
        };

        Assert.Equal("https://github.com/test/repo", link.Url);
        Assert.Equal("Test Repository", link.Title);
        Assert.Equal(LinkType.Repository, link.Type);
        Assert.Equal(projectId, link.ProjectId);
        Assert.Equal(2, link.Tags.Count);
        Assert.Equal("Useful repo", link.Notes);
    }
}
