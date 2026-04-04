namespace DevHub.Domain.Models;

public record IdeEntry(string Name, string Path)
{
    public IdeEntry() : this(string.Empty, string.Empty) { }
}
