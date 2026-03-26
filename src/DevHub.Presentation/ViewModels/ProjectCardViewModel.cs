using CommunityToolkit.Mvvm.ComponentModel;
using DevHub.Application.DTOs;
using DevHub.Domain.Enums;
using DevHub.Presentation.Base;

namespace DevHub.Presentation.ViewModels;

public partial class ProjectCardViewModel : BaseUserControlViewModel
{
    public ProjectDto Dto { get; }

    public ProjectCardViewModel(ProjectDto dto)
    {
        Dto = dto;
    }

    public Guid Id => Dto.Id;
    public string Name => Dto.Name;
    public string Path => Dto.Path;
    public string? Description => Dto.Description;
    public string? Notes => Dto.Notes;
    public ProgrammingLanguage Language => Dto.Language;
    public ProjectStatus Status => Dto.Status;
    public List<string> Tags => Dto.Tags;
    public string? PreferredIde => Dto.PreferredIde;
    public bool IsFavorite => Dto.IsFavorite;
    public DateTime UpdatedAt => Dto.UpdatedAt;

    public string StatusColor => Status switch
    {
        ProjectStatus.Active => "#4CAF50",
        ProjectStatus.Completed => "#2196F3",
        ProjectStatus.Paused => "#FF9800",
        ProjectStatus.Archived => "#9E9E9E",
        _ => "#000000"
    };

    public string LanguageIcon => Language switch
    {
        ProgrammingLanguage.CSharp => "🔷",
        ProgrammingLanguage.Python => "🐍",
        ProgrammingLanguage.Rust => "🦀",
        ProgrammingLanguage.JavaScript => "🟨",
        ProgrammingLanguage.TypeScript => "🔵",
        ProgrammingLanguage.Go => "🐹",
        ProgrammingLanguage.Java => "☕",
        ProgrammingLanguage.Cpp => "⚙️",
        _ => "📄"
    };

    public string FormattedUpdatedAt => UpdatedAt.ToString("dd.MM.yyyy HH:mm");
}
