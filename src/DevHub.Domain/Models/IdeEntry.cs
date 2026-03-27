namespace DevHub.Domain.Models;

public class IdeEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    public IdeEntry() { }

    public IdeEntry(string name, string path)
    {
        Name = name;
        Path = path;
    }
}
