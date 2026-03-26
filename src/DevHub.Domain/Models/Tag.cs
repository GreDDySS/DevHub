namespace DevHub.Domain.Models;

public class Tag : BaseModel
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string? _color;

    public string? Color
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }

    private int _usageCount;

    public int UsageCount
    {
        get => _usageCount;
        set => SetProperty(ref _usageCount, value);
    }
}
