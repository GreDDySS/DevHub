using CommunityToolkit.Mvvm.ComponentModel;

namespace DevHub.Infrastructure.Storage;

public class IdeEntry : ObservableObject
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _path = string.Empty;

    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    public IdeEntry() { }

    public IdeEntry(string name, string path)
    {
        _name = name;
        _path = path;
    }
}
