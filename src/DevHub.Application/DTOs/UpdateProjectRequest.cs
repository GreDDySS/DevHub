using DevHub.Domain.Enums;

namespace DevHub.Application.DTOs;

public record UpdateProjectRequest(
    string? Name = null,
    string? Description = null,
    string? Notes = null,
    ProjectStatus? Status = null,
    ProgrammingLanguage? Language = null,
    List<string>? Tags = null,
    string? PreferredIde = null,
    bool? IsFavorite = null,
    bool? IsHidden = null);
