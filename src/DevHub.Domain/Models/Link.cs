using DevHub.Domain.Enums;

namespace DevHub.Domain.Models;

public class Link : BaseModel
{
    private string _url = string.Empty;

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    private string? _title;

    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private LinkType _type = LinkType.Other;

    public LinkType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    private Guid? _projectId;

    public Guid? ProjectId
    {
        get => _projectId;
        set => SetProperty(ref _projectId, value);
    }

    private List<string> _tags = [];

    public List<string> Tags
    {
        get => _tags;
        set => SetProperty(ref _tags, value);
    }

    private string? _notes;

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    private DateTime _capturedAt = DateTime.UtcNow;

    public DateTime CapturedAt
    {
        get => _capturedAt;
        set => SetProperty(ref _capturedAt, value);
    }
}
