using DevHub.Domain.Enums;
using DevHub.Domain.Models;

namespace DevHub.Domain.Tests;

public class LinkExtendedTests
{
    [Fact]
    public void Link_Create_ThrowsOnEmptyUrl()
    {
        Assert.Throws<DomainException>(() => Link.Create(""));
        Assert.Throws<DomainException>(() => Link.Create("  "));
    }

    [Fact]
    public void Link_Create_ThrowsOnNonHttpUrl()
    {
        Assert.Throws<DomainException>(() => Link.Create("ftp://example.com"));
        Assert.Throws<DomainException>(() => Link.Create("file:///c:/test"));
    }

    [Fact]
    public void Link_Create_ThrowsOnMaxLengthExceeded()
    {
        var longUrl = "https://example.com/" + new string('a', 2049);
        Assert.Throws<DomainException>(() => Link.Create(longUrl));
    }

    [Fact]
    public void Link_Create_AllowsUrlAtMaxLength()
    {
        var maxUrl = "https://example.com/" + new string('a', 2048 - "https://example.com/".Length);
        var link = Link.Create(maxUrl);
        Assert.Equal(maxUrl, link.Url);
    }

    [Fact]
    public void Link_Create_SetsCapturedAt()
    {
        var before = DateTime.UtcNow;
        var link = Link.Create("https://example.com");
        var after = DateTime.UtcNow;

        Assert.True(link.CapturedAt >= before);
        Assert.True(link.CapturedAt <= after);
    }

    [Fact]
    public void Link_SetTitle_UpdatesTitle()
    {
        var link = Link.Create("https://example.com");
        link.SetTitle("My Title");
        Assert.Equal("My Title", link.Title);
    }

    [Fact]
    public void Link_SetTitle_TrimsWhitespace()
    {
        var link = Link.Create("https://example.com");
        link.SetTitle("  Trimmed  ");
        Assert.Equal("Trimmed", link.Title);
    }

    [Fact]
    public void Link_SetTitle_ThrowsOnMaxLengthExceeded()
    {
        var link = Link.Create("https://example.com");
        var longTitle = new string('a', 501);
        Assert.Throws<DomainException>(() => link.SetTitle(longTitle));
    }

    [Fact]
    public void Link_SetTitle_AllowsNull()
    {
        var link = Link.Create("https://example.com");
        link.SetTitle("title");
        link.SetTitle(null);
        Assert.Null(link.Title);
    }

    [Fact]
    public void Link_SetNotes_UpdatesNotes()
    {
        var link = Link.Create("https://example.com");
        link.SetNotes("some notes");
        Assert.Equal("some notes", link.Notes);
    }

    [Fact]
    public void Link_SetNotes_TrimsWhitespace()
    {
        var link = Link.Create("https://example.com");
        link.SetNotes("  trimmed  ");
        Assert.Equal("trimmed", link.Notes);
    }

    [Fact]
    public void Link_SetNotes_ThrowsOnMaxLengthExceeded()
    {
        var link = Link.Create("https://example.com");
        var longNotes = new string('a', 5001);
        Assert.Throws<DomainException>(() => link.SetNotes(longNotes));
    }

    [Fact]
    public void Link_SetNotes_AllowsNull()
    {
        var link = Link.Create("https://example.com");
        link.SetNotes("notes");
        link.SetNotes(null);
        Assert.Null(link.Notes);
    }

    [Fact]
    public void Link_SetProjectId_SetsValue()
    {
        var link = Link.Create("https://example.com");
        var id = Guid.NewGuid();
        link.SetProjectId(id);
        Assert.Equal(id, link.ProjectId);
    }

    [Fact]
    public void Link_SetProjectId_AllowsNull()
    {
        var link = Link.Create("https://example.com");
        link.SetProjectId(Guid.NewGuid());
        link.SetProjectId(null);
        Assert.Null(link.ProjectId);
    }

    [Fact]
    public void Link_SetType_UpdatesType()
    {
        var link = Link.Create("https://example.com");
        link.SetType(LinkType.YouTube);
        Assert.Equal(LinkType.YouTube, link.Type);
    }

    [Fact]
    public void Link_SetTags_ReplacesAllTags()
    {
        var link = Link.Create("https://example.com");
        link.AddTag("old");
        link.SetTags(["new1", "new2"]);
        Assert.Equal(2, link.Tags.Count);
        Assert.Contains("new1", link.Tags);
        Assert.Contains("new2", link.Tags);
    }

    [Fact]
    public void Link_SetTags_NullArgument_Throws()
    {
        var link = Link.Create("https://example.com");
        Assert.Throws<ArgumentNullException>(() => link.SetTags(null!));
    }

    [Fact]
    public void Link_SetTags_FiltersEmptyAndTrims()
    {
        var link = Link.Create("https://example.com");
        link.SetTags(["  a  ", "", "  b  "]);
        Assert.Equal(2, link.Tags.Count);
        Assert.Contains("a", link.Tags);
        Assert.Contains("b", link.Tags);
    }

    [Fact]
    public void Link_SetTags_FiltersLongTags()
    {
        var link = Link.Create("https://example.com");
        var longTag = new string('a', 51);
        link.SetTags(["short", longTag]);
        Assert.Single(link.Tags);
    }

    [Fact]
    public void Link_SetTags_DeduplicatesCaseInsensitive()
    {
        var link = Link.Create("https://example.com");
        link.SetTags(["API", "api", "Api"]);
        Assert.Single(link.Tags);
    }

    [Fact]
    public void Link_SetTags_LimitsToMaxCount()
    {
        var link = Link.Create("https://example.com");
        var tags = Enumerable.Range(0, 60).Select(i => $"tag{i}").ToList();
        link.SetTags(tags);
        Assert.Equal(50, link.Tags.Count);
    }

    [Fact]
    public void Link_AddTag_ThrowsOnEmpty()
    {
        var link = Link.Create("https://example.com");
        Assert.Throws<DomainException>(() => link.AddTag(""));
        Assert.Throws<DomainException>(() => link.AddTag("   "));
    }

    [Fact]
    public void Link_AddTag_ThrowsOnMaxLengthExceeded()
    {
        var link = Link.Create("https://example.com");
        var longTag = new string('a', 51);
        Assert.Throws<DomainException>(() => link.AddTag(longTag));
    }

    [Fact]
    public void Link_AddTag_ThrowsOnMaxTagsCountExceeded()
    {
        var link = Link.Create("https://example.com");
        for (int i = 0; i < 50; i++)
            link.AddTag($"tag{i}");
        Assert.Throws<DomainException>(() => link.AddTag("oneMore"));
    }

    [Fact]
    public void Link_AddTag_DeduplicatesCaseInsensitive()
    {
        var link = Link.Create("https://example.com");
        link.AddTag("API");
        link.AddTag("api");
        Assert.Single(link.Tags);
    }
}
