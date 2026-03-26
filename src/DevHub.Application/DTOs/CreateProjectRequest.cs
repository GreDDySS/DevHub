using DevHub.Domain.Enums;

namespace DevHub.Application.DTOs;

public record CreateProjectRequest(
    string Name,
    string Path,
    string? Description,
    ProgrammingLanguage Language);
