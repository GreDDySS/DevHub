using DevHub.Domain.Enums;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests;

public class LinkTests
{
    [Fact]
    public void Link_Create_ShouldInitializeWithCorrectValues()
    {
        var link = Link.Create("https://example.com", LinkType.Article);

        Assert.NotEqual(Guid.Empty, link.Id);
        Assert.Equal("https://example.com", link.Url);
        Assert.Equal(LinkType.Article, link.Type);
        Assert.Null(link.Title);
        Assert.Null(link.ProjectId);
        Assert.Empty(link.Tags);
        Assert.Null(link.Notes);
    }

    [Fact]
    public void Link_Create_ThrowsOnInvalidUrl()
    {
        Assert.Throws<DomainException>(() => Link.Create(""));
        Assert.Throws<DomainException>(() => Link.Create("not-a-url"));
        Assert.Throws<DomainException>(() => Link.Create("ftp://example.com"));
    }

    [Fact]
    public void Link_SetTitle_ShouldSetTitle()
    {
        var link = Link.Create("https://example.com");

        link.SetTitle("My Title");

        Assert.Equal("My Title", link.Title);
    }

    [Fact]
    public void Link_SetProjectId_ShouldSetProjectId()
    {
        var link = Link.Create("https://example.com");
        var projectId = Guid.NewGuid();

        link.SetProjectId(projectId);

        Assert.Equal(projectId, link.ProjectId);
    }

    [Fact]
    public void Link_SetType_ShouldChangeType()
    {
        var link = Link.Create("https://example.com");

        link.SetType(LinkType.Repository);

        Assert.Equal(LinkType.Repository, link.Type);
    }

    [Fact]
    public void Link_AddTag_ShouldAddUniqueTag()
    {
        var link = Link.Create("https://example.com");

        link.AddTag("git");
        link.AddTag("open-source");
        link.AddTag("GIT"); // duplicate

        Assert.Equal(2, link.Tags.Count);
    }

    [Fact]
    public void Link_SetTags_ShouldReplaceAllTags()
    {
        var link = Link.Create("https://example.com");
        link.AddTag("old");

        link.SetTags(["new1", "new2"]);

        Assert.Equal(2, link.Tags.Count);
    }
}
