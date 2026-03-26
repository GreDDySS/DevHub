using DevHub.Domain.Models;

namespace DevHub.Domain.Tests;

public class TagTests
{
    [Fact]
    public void Tag_ShouldInheritBaseModel()
    {
        var tag = new Tag();

        Assert.IsAssignableFrom<BaseModel>(tag);
        Assert.NotEqual(Guid.Empty, tag.Id);
    }

    [Fact]
    public void Tag_DefaultValues_ShouldBeCorrect()
    {
        var tag = new Tag();

        Assert.Equal(string.Empty, tag.Name);
        Assert.Null(tag.Color);
        Assert.Equal(0, tag.UsageCount);
    }

    [Fact]
    public void Tag_PropertiesShouldBeSettable()
    {
        var tag = new Tag
        {
            Name = "api",
            Color = "#FF5733",
            UsageCount = 5
        };

        Assert.Equal("api", tag.Name);
        Assert.Equal("#FF5733", tag.Color);
        Assert.Equal(5, tag.UsageCount);
    }
}
