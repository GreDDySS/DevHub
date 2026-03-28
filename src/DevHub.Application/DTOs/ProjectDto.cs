using DevHub.Domain.Enums;

namespace DevHub.Application.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string Path,
    string? Description,
    string? Notes,
    ProgrammingLanguage Language,
    ProjectStatus Status,
    List<string> Tags,
    string? PreferredIde,
    bool IsFavorite,
    bool IsHidden,
    DateTime UpdatedAt);
