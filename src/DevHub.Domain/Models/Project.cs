using DevHub.Domain.Enums;

namespace DevHub.Domain.Models;

public class Project : BaseModel
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

    private string? _description;

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string? _notes;

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    private ProgrammingLanguage _language = ProgrammingLanguage.Other;

    public ProgrammingLanguage Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
    }

    private ProjectStatus _status = ProjectStatus.Active;

    public ProjectStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    private List<string> _tags = [];

    public List<string> Tags
    {
        get => _tags;
        set => SetProperty(ref _tags, value);
    }

    private string? _preferredIde;

    public string? PreferredIde
    {
        get => _preferredIde;
        set => SetProperty(ref _preferredIde, value);
    }

    private bool _isFavorite;

    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetProperty(ref _isFavorite, value);
    }

    private bool _isHidden;

    public bool IsHidden
    {
        get => _isHidden;
        set => SetProperty(ref _isHidden, value);
    }

    private DateTime? _lastAccessedAt;

    public DateTime? LastAccessedAt
    {
        get => _lastAccessedAt;
        set => SetProperty(ref _lastAccessedAt, value);
    }

    private bool _autoStatusEnabled = true;

    public bool AutoStatusEnabled
    {
        get => _autoStatusEnabled;
        set => SetProperty(ref _autoStatusEnabled, value);
    }
}
